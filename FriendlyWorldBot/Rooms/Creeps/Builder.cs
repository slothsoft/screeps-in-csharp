using System.Linq;
using FriendlyWorldBot.Utils;
using ScreepsDotNet.API.World;
using static FriendlyWorldBot.Utils.IMemoryConstants;

namespace FriendlyWorldBot.Rooms.Creeps;

/// <summary>
/// Builder will try to repair buildings or build new ones. If both are not present, they will act like a miner.
/// </summary>
public class Builder : IJob {
    private const string TargetSeparator = ",";
    
    private readonly IGame _game;
    private readonly RoomCache _room;

    public Builder(IGame game, RoomCache room) {
        _game = game;
        _room = room;
    }

    public string Id => "builder";
    public string Icon => "🛠";
    public int WantedCreepCount => 3;

    public void Run(ICreep creep) {
        creep.Memory.TryGetBool(CreepIsBuilding, out var isBuilding);

        if (isBuilding) {
            RunInBuilderMode(creep);

            if (creep.Store.GetUsedCapacity(ResourceType.Energy) == 0) {
                // we cannot build any longer, so switch to miner mode
                creep.Memory.SetValue(CreepIsBuilding, false);
            }
        } else {
            RunInMinerMode(creep);

            if (creep.Store.GetFreeCapacity(ResourceType.Energy) == 0) {
                // we cannot mine any longer, so switch to builder mode
                creep.Memory.SetValue(CreepIsBuilding, true);
            }
        }
    }

    private void RunInBuilderMode(ICreep creep) {
        // if the creep already has a target, repair it until it's finished
        if (creep.Memory.TryGetString(CreepTarget, out var targetId)) {
            if (RunInRepairMode(creep, targetId)) {
                return;
            }
        }
        
        if (_room.Room.Memory.TryGetString(RoomBrokenStructures, out var brokenStructuresString)) {
            // if the room already has broken structures saved take one of these
            var brokenStructureIds = brokenStructuresString.Split(TargetSeparator).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            var brokenStructureId = brokenStructureIds.FirstOrDefault();
            if (brokenStructureId != null) {
                _room.Room.Memory.SetValue(RoomBrokenStructures, string.Join(TargetSeparator, brokenStructureIds.Where(i => i != brokenStructureId)));
                creep.Memory.SetValue(CreepTarget, brokenStructureId);
                if (RunInRepairMode(creep, brokenStructureId))
                {
                    return;
                }
            }
        } else {
            // find new broken structures if the room doesn't have a list anymore
            var repairAtPercent = _game.Memory.TryGetDouble(GameRepairAtPercent, out var value) ? value : GameRepairAtPercentDefault;
            var brokenStructures = _room.Room.Find<IStructure>().Where(s => s.Hits <= s.HitsMax * repairAtPercent).OrderBy(s => (double)s.Hits / s.HitsMax).ToArray();
            var bsStrings = string.Join(TargetSeparator, brokenStructures.Skip(1).Select(s => s.Id));
            _room.Room.Memory.SetValue(RoomBrokenStructures, string.Join(TargetSeparator, bsStrings));
            var structure = brokenStructures.FirstOrDefault();
            if (structure != null) {
                creep.Memory.SetValue(CreepTarget, structure.Id);
                RunInRepairMode(creep, structure);
                return;
            }
        }
        
        // so if there are no broken structures... build stuff
        RunInBuildMode(creep);
    }

    private bool RunInRepairMode(ICreep creep, string targetId) {
        var structure = _room.Room.Find<IStructure>().SingleOrDefault(s => s.Id.ToString() == targetId);
        if (structure != null) {
            RunInRepairMode(creep, structure);
            return true;
        } 
        // remove the broken ID
        creep.Memory.SetValue(CreepTarget, string.Empty);
        return false;
    }

    private static void RunInRepairMode(ICreep creep, IStructure target) {
        var transferResult = creep.Repair(target);
        if (transferResult == CreepRepairResult.NotInRange) {
            creep.BetterMoveTo(target.RoomPosition);
        } else if (transferResult == CreepRepairResult.Ok) {
            if (target.Hits >= target.HitsMax) {
                creep.Memory.SetValue(CreepTarget, string.Empty);
            }
        }else {
            creep.LogInfo($"unexpected result when depositing to {target} ({transferResult})");
        }
    }

    private void RunInBuildMode(ICreep creep) {
        var constructionSite = creep.Room!.Find<IConstructionSite>().FirstOrDefault();
        if (constructionSite != null) {
            var transferResult = creep.Build(constructionSite);
            if (transferResult == CreepBuildResult.NotInRange) {
                creep.BetterMoveTo(constructionSite.RoomPosition);
            } else if (transferResult != CreepBuildResult.Ok) {
                creep.LogInfo($"unexpected result when depositing to {constructionSite} ({transferResult})");
            }
        } else {
            // no construction sites, so the builder switches to miner to be useful
            creep.PutIntoStorage(_room);
        }
    }

    private void RunInMinerMode(ICreep creep) {
        creep.MineResource(_room);
    }
}