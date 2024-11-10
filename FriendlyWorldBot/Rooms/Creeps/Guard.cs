using System.Collections.Generic;
using System.Linq;
using FriendlyWorldBot.Rooms.Structures;
using FriendlyWorldBot.Utils;
using ScreepsDotNet.API.World;
using static FriendlyWorldBot.Utils.IMemoryConstants;

namespace FriendlyWorldBot.Rooms.Creeps;

/// <summary>
/// A guard lives inside a rampart and attacks enemies.
/// </summary>
public class Guard : IJob {
    internal const string JobId = "guard";
    private static readonly BodyPartGroup[] GuardBodyPartGroups = [ 
        BodyPartGroup.Variable(0, 5, BodyPartType.Tough), 
        BodyPartGroup.Variable(1, 3, BodyPartType.Attack), 
        BodyPartGroup.Variable(1, 3, BodyPartType.Move), 
        BodyPartGroup.Fixed(1, BodyPartType.Carry), 
    ];

    private readonly IGame _game;
    private readonly RoomCache _room;

    public Guard(IGame game, RoomCache room) {
        _game = game;
        _room = room;
    }

    public string Id => JobId;
    public string Icon => "🛡️";
    public int WantedCreepCount => _room.Ramparts.Count();
    public IEnumerable<BodyPartGroup> BodyPartGroups => GuardBodyPartGroups;
    public int Priority => 100;

    public void Run(ICreep creep)
    {
        // first priority: suicide
        if (!creep.Body.Any(p => p.Type is BodyPartType.Attack or BodyPartType.RangedAttack)) {
            // we can't attack any longer, so suicide
            creep.Memory.SetValue(CreepSuicide, false);
        }
        if (creep.MoveToRecycleAtSpawnIfNecessary(_room)) {
            return;
        }
        
        // second priority: if we already have a target, follow it, else try to attack enemies
        if (creep.MoveToAttackInSameRoom()) {
            return;
        }
        
        // third priority: collect energy from corpses or ruins?
        if (creep.Store.GetFreeCapacity(ResourceType.Energy) > 0) {
            if (!creep.MoveToPickupLostResources(_room)) {
                MoveToDrop(creep);
            }
        } else {
            MoveToDrop(creep);
        }
        
        // last priority: move to rampart
        // var rampart = _room.Ramparts.FirstOrDefault();
        // if (rampart != null) {
        //     creep.BetterMoveTo(rampart.RoomPosition);
        // }
    }

    private void MoveToDrop(ICreep creep) {
        var tower = _room.Towers.FindNearest(creep.LocalPosition);
        if (tower == null) return;
        if (tower.Store.GetFreeCapacity(ResourceType.Energy) > 0) {
            var transferResult = creep.Transfer(tower, ResourceType.Energy);
            if (transferResult == CreepTransferResult.NotInRange) {
                creep.BetterMoveTo(tower.RoomPosition);
                return;
            } 
            if (transferResult != CreepTransferResult.Ok && transferResult != CreepTransferResult.NotEnoughResources && transferResult != CreepTransferResult.Full) {
                creep.LogInfo($"unexpected result when transfering to {tower} ({transferResult})");
            }
            return;
        }

        var (container, _) = _room.FindOrCreateConstructionSite<IStructureContainer>(StructureTypes.GraveyardContainer);
        if (container != null) {
            creep.MoveToTransferInto(container);
        }
    }
}