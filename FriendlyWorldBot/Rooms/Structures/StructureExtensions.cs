using System.Collections.Generic;
using System.Linq;
using FriendlyWorldBot.Rooms.Creeps;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Rooms.Structures;

public static class StructureExtensions {
    public static TStructure? FindNearest<TStructure>(this IEnumerable<TStructure> structures, Position pos)
        where TStructure : IRoomObject
    {
        return structures
            .Where(static x => x.Exists)
            .MinBy(x => x.LocalPosition.LinearDistanceTo(pos));
    }
    
    // TOWER - ATTACK
    
    public static bool AttackNearestEnemy(this IStructureTower attacker)
    {
        if (attacker.Store.GetUsedCapacity(ResourceType.Energy) == 0) {
            // we can't fire right now
            return false;
        }

        var enemy = attacker.FindNearestEnemy(StructureType.Tower.CollectionName);
        if (enemy == null) {
            return false;
        }
        var result = attacker.Attack(enemy);
        if (result != TowerActionResult.Ok) {
            return false;
        }
        attacker.SetUserData(enemy);
        return true;
    }
}