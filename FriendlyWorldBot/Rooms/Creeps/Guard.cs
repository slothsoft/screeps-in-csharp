using System.Collections.Generic;
using System.Linq;
using FriendlyWorldBot.Rooms.Structures;
using FriendlyWorldBot.Utils;
using ScreepsDotNet.API.World;
using static FriendlyWorldBot.Utils.IMemoryConstants;

namespace FriendlyWorldBot.Rooms.Creeps;

/// <summary>
/// A guard lives inside a rampart and attacks enemies or heals my game objects.
/// </summary>
public class Guard : IJob {
    
    private static readonly BodyPartGroup[] GuardBodyPartGroups = [ 
        BodyPartGroup.Variable(1, 3, BodyPartType.Attack), 
        BodyPartGroup.Variable(0, 10, BodyPartType.Tough), 
        BodyPartGroup.Fixed(1, BodyPartType.Move), 
    ];

    
    private readonly IGame _game;
    private readonly RoomCache _room;

    public Guard(IGame game, RoomCache room) {
        _game = game;
        _room = room;
    }

    public string Id => "guard";
    public string Icon => "🛡️";
    public int WantedCreepCount => _room.Ramparts.Count;
    public IEnumerable<BodyPartGroup> BodyPartGroups => GuardBodyPartGroups;

    public void Run(ICreep creep)
    {
        // first priority: if we already have a target, follow it
        if (creep.Memory.TryGetString(CreepTarget, out var targetId) && !string.IsNullOrEmpty(targetId)) {
            var targetEnemy = _room.Room.Find<ICreep>(my: false).SingleOrDefault(c => c.Id == targetId);
            if (Attack(creep, targetEnemy)) {
                return;
            }
        }
        
        // second priority: attack foes!
        var enemy = _room.Room.Find<ICreep>(my: false).FindNearest(creep.LocalPosition);
        if (enemy != null) {
            if (Attack(creep, enemy)) {
                return;
            }
        }
        
        // third priority: collect energy from corpses or ruins?
        // Check energy storage
        if (creep.Store.GetFreeCapacity(ResourceType.Energy) > 0) {
            creep.HarvestResource(_room);
        } else {
            creep.PutIntoStorage(_room);
        }
        
        // last priority: move to rampart
        // var rampart = _room.Ramparts.FirstOrDefault();
        // if (rampart != null) {
        //     creep.BetterMoveTo(rampart.RoomPosition);
        // }
    }

    private bool Attack(ICreep creep, ICreep? enemy) {
        if (enemy == null) {
            creep.Memory.SetValue(CreepTarget, string.Empty);
            return false;
        }

        creep.Memory.SetValue(CreepTarget, enemy.Id);
        var attackResult = creep.Attack(enemy);
        if (attackResult == CreepAttackResult.NotInRange) {
            enemy.BetterMoveTo(enemy.RoomPosition);
        } else if (attackResult != CreepAttackResult.Ok) {
            enemy.LogInfo($"unexpected result when harvesting {creep} ({attackResult})");
        }
        
        // if enemy was attacked and is now done, increment kill count
        if (!enemy.Exists) {
            _room.Room.Memory.IncrementKillCount(Id);
            _game.Memory.IncrementKillCount(Id);
        }

        return true;
    }
}