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
        BodyPartGroup.Variable(1, 3, BodyPartType.Attack), 
        BodyPartGroup.Variable(0, 10, BodyPartType.Tough), 
        BodyPartGroup.Fixed(1, BodyPartType.Move, BodyPartType.Carry), 
    ];

    private readonly IGame _game;
    private readonly RoomCache _room;

    public Guard(IGame game, RoomCache room) {
        _game = game;
        _room = room;
    }

    public string Id => JobId;
    public string Icon => "🛡️";
    public int WantedCreepCount => _room.Ramparts.Count;
    public IEnumerable<BodyPartGroup> BodyPartGroups => GuardBodyPartGroups;

    public void Run(ICreep creep)
    {
        // first priority: if we already have a target, follow it, else try to attack enemies
        if (creep.MoveToAttackInSameRoom()) {
            return;
        }

        // second priority: suicide
        if (creep.MoveToRecycleAtSpawnIfNecessary(_room)) {
            return;
        }
        
        // third priority: collect energy from corpses or ruins?
        if (creep.Store.GetFreeCapacity(ResourceType.Energy) > 0) {
            if (!creep.MoveToPickupLostResources(_room)) {
                creep.MoveToTransferIntoStorage(_room);
            }
        } else {
            creep.MoveToTransferIntoStorage(_room);
        }
        
        // last priority: move to rampart
        // var rampart = _room.Ramparts.FirstOrDefault();
        // if (rampart != null) {
        //     creep.BetterMoveTo(rampart.RoomPosition);
        // }
    }
}