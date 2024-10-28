﻿using System.Collections.Generic;
using System.Linq;
using FriendlyWorldBot.Paths;
using FriendlyWorldBot.Utils;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;
using static FriendlyWorldBot.Utils.IMemoryConstants;

namespace FriendlyWorldBot.Rooms.Creeps;

/// <summary>
/// A pioneer is used to claim other rooms and prepare the new settlement.
///
/// See https://wiki.screepspl.us/index.php/Claming_new_room
/// - send a creep with at least one CLAIM body part to an unowned room
/// - have this creep successfully perform the .claimController() action
/// - place a spawn construction site
/// - bring builders from at least one of the rooms you already own
/// - build the spawn
/// - keep the controller from downgrading and losing a Room Control Level (RCL)
/// </summary>
public class Pioneer : IJob {
    
    private static readonly BodyPartGroup[] PioneerBodyPartGroups = [ 
        BodyPartGroup.Variable(1, 3, BodyPartType.Move), 
        BodyPartGroup.Variable(0, 10, BodyPartType.Tough), 
        BodyPartGroup.Fixed(1, BodyPartType.Claim, BodyPartType.Work, BodyPartType.Carry), 
    ];
    private static readonly BodyPartGroup[] PioneerWithoutClaimBodyPartGroups = [ 
        BodyPartGroup.Variable(1, 5, BodyPartType.Move), 
        BodyPartGroup.Variable(0, 10, BodyPartType.Tough), 
        BodyPartGroup.Fixed(1, BodyPartType.Work, BodyPartType.Carry), 
    ];
    private const string FlagNewSettlement = "[NEW]";
    
    private readonly IGame _game;
    private readonly RoomCache _room;
    private readonly CreepManager _creepManager;
    
    private readonly IDictionary<string, RoomCache> _otherRoomCaches = new Dictionary<string, RoomCache>();
    
    public Pioneer(IGame game, RoomCache room, CreepManager creepManager) {
        _game = game;
        _room = room;
        _creepManager = creepManager;
    }

    public string Id => "pioneer";
    public string Icon => "\ud83e\udd20";
    public int WantedCreepCount => _game.Flags.Count(f => f.Key.StartsWith(FlagNewSettlement));
    public int Priority => 50;
    public IEnumerable<BodyPartGroup> BodyPartGroups {
        get {
            var flagName = FetchNextFlagName();
            if (flagName != null && _game.Flags[flagName].Room != null) {
                // if we can see the room, it's ours. so we don't need CLAIM any more
                return PioneerWithoutClaimBodyPartGroups;
            }
            return PioneerBodyPartGroups;
        }
    }

    public void OnCreepSpawned(ICreep creep) {
       var notUsedFlagName = FetchNextFlagName();
       if (notUsedFlagName != null) {
           creep.Memory.SetValue(CreepTarget, notUsedFlagName);
       }
    }

    private string? FetchNextFlagName() {
        var usedFlagNames = _creepManager
            .GetCreeps(Id)
            .Select(c => c.Memory.TryGetString(CreepTarget, out var flagName) ? flagName : null)
            .OfType<string>().ToArray();
        var actualFlagNames = _game.Flags
            .Select(f => f.Key)
            .Where(s => s.StartsWith(FlagNewSettlement))
            .ToList();
        foreach (var usedFlagName in usedFlagNames) {
            actualFlagNames.Remove(usedFlagName);
        }
        return actualFlagNames.FirstOrDefault();
    }

    public void Run(ICreep creep)
    {
        // o. recycle if flag was not set correctly
        if (!creep.Memory.TryGetString(CreepTarget, out var flagName) || string.IsNullOrEmpty(flagName)) {
            creep.Memory.SetValue(CreepSuicide, true);
            if (creep.MoveToRecycleAtSpawnIfNecessary(_room))
                return;
        }
        var log = 0;
        creep.Memory.SetValue(CreepPioneerLog + log++, $"Flag {flagName} was fond");
        
        // 1. send a creep to an unowned room
        var flag = _game.Flags!.GetValueOrDefault(flagName);
        if (flag == null) {
            return;
        }
        if (creep.RoomPosition.RoomName != flag.RoomPosition.RoomName) {
            creep.BetterMoveTo(flag.RoomPosition);
            return;
        }
        creep.Memory.SetValue(CreepPioneerLog + log++, $"Went to room {flag.RoomPosition.RoomName}");

        // 2. have this creep successfully perform the claimController() action
        // NOTE: on this point we are in a different room than the one the creep spawned in
        var controller = creep.Room?.Controller;
        if (controller == null) {
            creep.LogError($"no controller found in {creep.Room}");
            return;
        }
        var room = creep.Room!;
        if (!controller.My) {
            var claimResult = creep.ClaimController(controller);
            if (claimResult == CreepClaimControllerResult.NotInRange) {
                creep.BetterMoveTo(controller.RoomPosition);
            } else if (claimResult != CreepClaimControllerResult.Ok) {
                creep.LogError($"unexpected result when claiming {controller} ({claimResult})");
            }
            return;
        }
        creep.Memory.SetValue(CreepPioneerLog + log++, $"The controller {controller.Id} of {room.Name} room was claimed");
        
        // 3. place a spawn construction site
        var spawnName = flagName![(FlagNewSettlement.Length + 1)..];
        var spawn = room.Find<IRoomObject>().FirstOrDefault(s => s is IStructureSpawn || (s is IConstructionSite cs && cs.StructureType == typeof(IStructureSpawn)));
        if (spawn == null) {
            var path = flag.Memory.TryGetPath(FlagSpawnCoordinate);
            if (!path.ToPositions().Any()) {
                path = CalculateSpawnCoordinate(creep.Room!);
                flag.Memory.SetValue(FlagSpawnCoordinate, path.Stringify());
            }

            var position = path.ToPositions().Single();
            var spiral = new Position(0, 0);
            var number = 1;
            while (room.CreateConstructionSite<IStructureSpawn>(
                       new Position(position.X + spiral.X, position.Y + spiral.Y),
                       spawnName
                   ) != RoomCreateConstructionSiteResult.InvalidTarget) {
                spiral = number.ToUlamSpiral().Last();
                number++;
            }

            spawn = room.Find<IConstructionSite>().FirstOrDefault(s => s.StructureType == typeof(IStructureSpawn));
        } 
        creep.Memory.SetValue(CreepPioneerLog + log++, $"Construction site for {spawnName} of {room.Name} was set down");
        
        // 4. build this spawn object
        if (spawn is IConstructionSite constructionSite) {
            creep.Memory.TryGetBool(CreepIsBuilding, out var isBuilding);
            if (isBuilding) {
                creep.MoveToBuild(constructionSite);

                if (creep.Store.GetUsedCapacity(ResourceType.Energy) == 0) {
                    // we cannot build any longer, so switch to harvester mode
                    creep.Memory.SetValue(CreepIsBuilding, false);
                }
            } else {
                creep.Memory.SetValue(CreepTempTarget, string.Empty);
                creep.MoveToHarvestInRoom(GetRoomCache(room));

                if (creep.Store.GetFreeCapacity(ResourceType.Energy) == 0) {
                    // we cannot mine any longer, so switch to builder mode
                    creep.Memory.SetValue(CreepIsBuilding, true);
                }
            }
            return;
        }
        creep.Memory.SetValue(CreepPioneerLog + log++, $"Spawn of {room.Name} was constructed");
        
        // 5. Remove the flag so no more pioneers are sent
        // flag.Remove();
        creep.Memory.SetValue(CreepPioneerLog + log++, $"Flag {flagName} was removed");
        _otherRoomCaches.Remove(room.Name);

        // 6. keep the controller from downgrading and losing a Room Control Level (RCL)
        creep.MoveAsUpgrader(GetRoomCache(room));
        creep.Memory.SetValue(CreepPioneerLog + log, $"Upgrading {room.Name}...");
    }

    private RoomCache GetRoomCache(IRoom room) {
        return _otherRoomCaches.GetOrCreate(room.Name, _ => new RoomCache(room));
    }

    private static IPath CalculateSpawnCoordinate(IRoom room) {
        var relevantGameObjects = room.Find<ISource>().OfType<IRoomObject>().Concat(room.Find<IMineral>()).Concat(room.Find<IStructureController>()).ToArray();
        return new Point((int) relevantGameObjects.Average(o => o.LocalPosition.X), (int) relevantGameObjects.Average(o => o.LocalPosition.Y));
    }
}