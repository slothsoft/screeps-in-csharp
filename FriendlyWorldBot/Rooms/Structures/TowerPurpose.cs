using System.Linq;
using FriendlyWorldBot.Rooms.Creeps;
using FriendlyWorldBot.Utils;
using ScreepsDotNet.API.World;
using static FriendlyWorldBot.Utils.IMemoryConstants;

namespace FriendlyWorldBot.Rooms.Structures;

public class TowerPurpose : IPurpose {
    
    public void Run(IStructureTower tower) {
        if (tower.Store.GetUsedCapacity(ResourceType.Energy) == 0) {
            // we can't fire right now
            return;
        }
        
        // first priority: if we already have a target, follow it
        if (tower.GetMemory().TryGetString(CreepTarget, out var targetId) && !string.IsNullOrEmpty(targetId)) {
            var targetEnemy = tower.Room!.Find<ICreep>(my: false).SingleOrDefault(c => c.Id.ToString() == targetId);
            if (targetEnemy == null) {
                // no target any more, so we killed it
                tower.Room!.Memory.IncrementKillCount("tower");
                tower.GetMemory().IncrementKillCount("tower");
            } else {
                var result = tower.Attack(targetEnemy);
                return;
            }
            tower.GetMemory().SetValue(CreepTarget, string.Empty);
        }

        // second priority: attack foes!
        var enemy = tower.Room!.Find<ICreep>(my: false).SingleOrDefault(c => c.Id.ToString() == targetId);
        if (enemy != null) {
            var result = tower.Attack(enemy);
        }
    }
}