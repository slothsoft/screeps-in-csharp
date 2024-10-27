using System.Collections.Generic;
using System.Linq;
using FriendlyWorldBot.Utils;
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
        BodyPartGroup.Fixed(1, BodyPartType.Claim), 
    ];
    private const string FlagNewSettlement = "[NEW]";
    
    private readonly IGame _game;
    private readonly CreepManager _creepManager;

    public Pioneer(IGame game, CreepManager creepManager) {
        _game = game;
        _creepManager = creepManager;
    }

    public string Id => "pioneer";
    public string Icon => "\ud83e\udd20";
    public int WantedCreepCount => 0; //_game.Flags.Count(f => f.Key.StartsWith(FlagNewSettlement));
    public IEnumerable<BodyPartGroup> BodyPartGroups => PioneerBodyPartGroups;

    public void OnCreepSpawned(ICreep creep) {
       var usedFlagNames = _creepManager.GetCreeps(Id).Select(c => {
            if (creep.Memory.TryGetString(CreepTarget, out var flagName)) {
                return flagName;
            }
            return null;
        }).OfType<string>().ToArray();
       var actualFlagNames = _game.Flags.Select(f => f.Key).Where(s => s.StartsWith(FlagNewSettlement)).ToList();
       foreach (var usedFlagName in usedFlagNames) {
           actualFlagNames.Remove(usedFlagName);
       }
       var notUsedFlagName = actualFlagNames.FirstOrDefault();
       if (notUsedFlagName != null) {
           creep.Memory.SetValue(CreepTarget, notUsedFlagName);
       }
    }

    public void Run(ICreep creep)
    {
        // o. recycle if flag was not set correctly
        if (!creep.Memory.TryGetString(CreepTarget, out var flagName) || string.IsNullOrEmpty(flagName)) {
            return;
        }
        
        // 1. send a creep to an unowned room
        var flag = _game.Flags.GetValueOrDefault(flagName);
        if (flag == null) {
            return;
        }

        if (creep.RoomPosition.RoomName != flag.RoomPosition.RoomName) {
            creep.BetterMoveTo(flag.RoomPosition);
            return;
        }

        // 2. have this creep successfully perform the claimController() action
        // NOTE: on this point we are in a different room than the one the creep spawned in
        var controller = creep.Room?.Controller;
        if (controller == null) {
            creep.LogError($"no controller found in {creep.Room}");
            return;
        }
        if (!controller.My) {
            var claimResult = creep.ClaimController(controller);
            if (claimResult == CreepClaimControllerResult.NotInRange) {
                creep.BetterMoveTo(controller.RoomPosition);
            } else if (claimResult != CreepClaimControllerResult.Ok) {
                creep.LogError($"unexpected result when claiming {controller} ({claimResult})");
            }
        }
        
        // 3. place a spawn construction site
        
        
        /// - bring builders from at least one of the rooms you already own
        /// - build the spawn
        /// - keep the controller from downgrading and losing a Room Control Level (RCL)
    }
}