using System;
using System.Collections.Generic;
using System.Linq;
using FriendlyWorldBot.Utils;
using ScreepsDotNet.API.World;
using static FriendlyWorldBot.Utils.IMemoryConstants;

namespace FriendlyWorldBot.Rooms.Creeps;

/// <summary>
/// Builder will try to repair buildings or build new ones. If both are not present, they will act like a harvester.
/// </summary>
public class Builder : IJob {
    private const string TargetSeparator = ",";
     
    // TODO: maybe separate builder from maintainer and self-destruct builder when not needed
    
    private readonly IGame _game;
    private readonly RoomCache _room;

    public Builder(IGame game, RoomCache room) {
        _game = game;
        _room = room;
    }

    public string Id => "builder";
    public string Icon => "🏗️";
    public int WantedCreepCount => 3;
    public IEnumerable<BodyPartGroup> BodyPartGroups => IJob.DefaultBodyPartGroups;
    public int Priority => 40;

    public void Run(ICreep creep) {
        creep.Memory.TryGetBool(CreepIsBuilding, out var isBuilding);

        if (isBuilding) {
            RunInBuilderMode(creep);

            if (creep.Store.GetUsedCapacity(ResourceType.Energy) == 0) {
                // we cannot build any longer, so switch to harvester mode
                creep.Memory.SetValue(CreepIsBuilding, false);
            }
        } else
        {
            creep.Memory.SetValue(CreepTempTarget, string.Empty);
            creep.MoveToHarvestInRoom(_room);

            if (creep.Store.GetFreeCapacity(ResourceType.Energy) == 0) {
                // we cannot mine any longer, so switch to builder mode
                creep.Memory.SetValue(CreepIsBuilding, true);
            }
        }
    }

    private void RunInBuilderMode(ICreep creep) {
        var repairWallsAtPercent = _game.Memory.TryGetDouble(GameRepairWallsAtPercent, out var vw) ? vw : GameRepairWallsAtPercentDefault;
        
        // if the creep already has a target, repair it until it's finished
        if (creep.Memory.TryGetString(CreepTarget, out var targetId) && !string.IsNullOrEmpty(targetId)) {
            if (RunInRepairMode(creep, targetId, repairWallsAtPercent)) {
                return;
            }
        }
        
        // walls are a temp target that resets when the creep has no energy anymore
        if (creep.Memory.TryGetString(CreepTempTarget, out var tempTargetId) && !string.IsNullOrEmpty(tempTargetId)) {
            if (RunInRepairMode(creep, tempTargetId, repairWallsAtPercent)) {
                return;
            }
        }
        
        if (_room.Room.Memory.TryGetString(RoomBrokenStructures, out var brokenStructuresString) && !string.IsNullOrWhiteSpace(brokenStructuresString)) {
            // if the room already has broken structures saved take one of these
            var brokenStructureIds = brokenStructuresString.Split(TargetSeparator).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            var brokenStructureId = brokenStructureIds.FirstOrDefault();
            if (brokenStructureId != null) {
                _room.Room.Memory.SetValue(RoomBrokenStructures, string.Join(TargetSeparator, brokenStructureIds.Where(i => i != brokenStructureId)));
                creep.Memory.SetValue(CreepTarget, brokenStructureId);
                if (RunInRepairMode(creep, brokenStructureId, repairWallsAtPercent))
                {
                    return;
                }
            }
        } else {
            // find new broken structures if the room doesn't have a list anymore
            var repairStructuresAtPercent = _game.Memory.TryGetDouble(GameRepairStructuresAtPercent, out var vs) ? vs : GameRepairStructuresAtPercentDefault;
            var brokenStructures = _room.AllStructures
                .Where(s => s is not IStructureController)
                .Where(s => s is IStructureWall or IStructureRampart
                    ? s.Hits <= s.HitsMax * repairWallsAtPercent // walls are repaired only when they are REALLY critical
                    : s.Hits <= s.HitsMax * repairStructuresAtPercent
                ).OrderBy(s => s is IStructureWall or IStructureRampart 
                ? (double) s.Hits / (s.HitsMax * repairWallsAtPercent) //the max for the percentage should be lower
                : (double) s.Hits / s.HitsMax
                ).ToArray();
            var bsStrings = string.Join(TargetSeparator, brokenStructures.Skip(1).Select(s => s.Id));
            _room.Room.Memory.SetValue(RoomBrokenStructures, string.Join(TargetSeparator, bsStrings));
            var structure = brokenStructures.FirstOrDefault();
            if (structure != null) {
                creep.Memory.SetValue(CreepTarget, structure.Id);
                RunInRepairMode(creep, structure, repairWallsAtPercent);
                return;
            }
        }
        
        // so if there are no broken structures... build stuff
        RunInBuildMode(creep, repairWallsAtPercent);
    }

    private bool RunInRepairMode(ICreep creep, string targetId, double repairWallsAtPercent) {
        var structure = _room.AllStructures.SingleOrDefault(s => Equals(s.Id.ToString(), targetId));
        if (structure != null) {
            RunInRepairMode(creep, structure, repairWallsAtPercent);
            return true;
        } 
        // remove the broken ID
        creep.LogError("Could not find structure " + targetId);
        creep.Memory.SetValue(CreepTarget, string.Empty);
        return false;
    }

    private static void RunInRepairMode(ICreep creep, IStructure target, double repairWallsAtPercent) {
        var transferResult = creep.Repair(target);
        if (transferResult == CreepRepairResult.NotInRange) {
            creep.BetterMoveTo(target.RoomPosition);
        } else if (transferResult == CreepRepairResult.Ok) {
            // we need to block walls from being repaired a lot earlier, since they have 300.000.000 hit points
            if (target.Hits >= target.HitsMax || (target is IStructureWall && target.Hits >= target.HitsMax * repairWallsAtPercent)) {
                creep.Memory.SetValue(CreepTarget, string.Empty);
            }
        }else {
            creep.LogInfo($"unexpected result when depositing to {target} ({transferResult})");
        }
    }

    private void RunInBuildMode(ICreep creep, double repairWallsAtPercent) {
        var constructionSite = creep.Room!.Find<IConstructionSite>().FirstOrDefault();
        if (constructionSite != null) {
            creep.MoveToBuild(constructionSite);
        } else {
            // if there is nothing else to do... top off the walls
            var wall = _room.Room.Find<IStructure>()
                .Where(w => w is IStructureWall or IStructureRampart) 
                .Where(w => w.Hits < w.HitsMax)
                .MinBy(s => (double)s.Hits / s.HitsMax);
            if (wall == null) {
                // no construction sites, no walls, so the builder switches to harvester to be useful
                creep.MoveToTransferIntoStorage(_room);
            } else {
                creep.Memory.SetValue(CreepTempTarget, wall.Id);
                RunInRepairMode(creep, wall, repairWallsAtPercent);
            }
        }
    }
}