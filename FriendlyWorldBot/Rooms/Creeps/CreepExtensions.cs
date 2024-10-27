using System.Linq;
using FriendlyWorldBot.Rooms.Structures;
using FriendlyWorldBot.Utils;
using ScreepsDotNet.API.World;
using static FriendlyWorldBot.Utils.IMemoryConstants;

namespace FriendlyWorldBot.Rooms.Creeps;

public static class CreepExtensions {

    internal static void MoveToHarvestInRoom(this ICreep creep, RoomCache room) {
        var source = creep.FindAssignedResource(room);
        if (source == null) {
            return;
        }

        var harvestResult = creep.Harvest(source);
        if (harvestResult == CreepHarvestResult.NotInRange) {
            creep.BetterMoveTo(source.RoomPosition);
        } else if (harvestResult != CreepHarvestResult.Ok) {
            creep.LogInfo($"unexpected result when harvesting {source} ({harvestResult})");
        }
    }

    internal static bool MoveToRecycleAtSpawnIfNecessary(this ICreep creep, RoomCache room) {
        if (creep.Memory.TryGetBool(CreepSuicide, out var suicide) && suicide) {
            var nearestSpawn = room.FindNearestSpawn(creep.LocalPosition);
            if (nearestSpawn != null) {
                var result = nearestSpawn.RecycleCreep(creep);
                if (result == RecycleCreepResult.NotInRange) {
                    creep.BetterMoveTo(nearestSpawn.RoomPosition);
                } else if (result != RecycleCreepResult.Ok) {
                    return false;
                }

                return true;
            }

            creep.LogError($"could not find spawn when recycling {creep}");
            return false;
        }

        return false;
    }

    internal static bool MoveToPickupLostResources(this ICreep creep, RoomCache room) {
        var energySource = room.Room
            // concat the sources of the undertaking so we can weigh them
            .Find<ITombstone>().Select(t => (Amount: t.Store.GetUsedCapacity(ResourceType.Energy), RoomObject: (IRoomObject) t))
            .Concat(room.Room.Find<IRuin>().Select(r => (Amount: r.Store.GetUsedCapacity(ResourceType.Energy), RoomObject: (IRoomObject) r)))
            .Concat(room.Room.Find<IResource>().Select(r => (Amount: (int?) r.Amount, RoomObject: (IRoomObject) r)))
            // now sort by distance and amount (very rough estimate for how much energy will be left when the undertaker arrives)
            .Select(o => (o.Amount, GameObject: o.RoomObject, Weight: o.Amount - creep.LocalPosition.LinearDistanceTo(o.RoomObject.LocalPosition)))
            .Where(o => o.Weight > 0)
            .OrderByDescending(o => o.Weight)
            .Select(o => o.GameObject)
            .FirstOrDefault();
        if (energySource != null) {
            switch (energySource) {
                case IResource s:
                    return creep.MoveToPickup(s);
                case IRuin r:
                    return creep.MoveToWithdraw(r);
                case ITombstone t:
                    return creep.MoveToWithdraw(t);
            }
        }

        return false;
    }

    internal static bool MoveToPickup(this ICreep creep, IResource target) {
        var result = creep.Pickup(target);
        if (result == CreepPickupResult.NotInRange) {
            creep.BetterMoveTo(target.RoomPosition);
            return true;
        }

        if (result != CreepPickupResult.Ok) {
            creep.LogInfo($"unexpected result when picking up {target} ({result})");
        }

        return result == CreepPickupResult.Ok;
    }

    internal static bool MoveToWithdraw(this ICreep creep, IStructure target, ResourceType resourceType = ResourceType.Energy, int? amount = null) {
        var result = creep.Withdraw(target, resourceType, amount);
        if (result == CreepWithdrawResult.NotInRange) {
            creep.BetterMoveTo(target.RoomPosition);
            return true;
        }

        if (result != CreepWithdrawResult.Ok) {
            creep.LogInfo($"unexpected result when withdrawing from {target} ({result})");
        }

        return result == CreepWithdrawResult.Ok;
    }

    internal static bool MoveToWithdraw(this ICreep creep, IRuin target, ResourceType resourceType = ResourceType.Energy, int? amount = null) {
        var result = creep.Withdraw(target, resourceType, amount);
        if (result == CreepWithdrawResult.NotInRange) {
            creep.BetterMoveTo(target.RoomPosition);
            return true;
        }

        if (result != CreepWithdrawResult.Ok) {
            creep.LogInfo($"unexpected result when withdrawing from {target} ({result})");
        }

        return result == CreepWithdrawResult.Ok;
    }

    internal static bool MoveToWithdraw(this ICreep creep, ITombstone target, ResourceType resourceType = ResourceType.Energy, int? amount = null) {
        var result = creep.Withdraw(target, resourceType, amount);
        if (result == CreepWithdrawResult.NotInRange) {
            creep.BetterMoveTo(target.RoomPosition);
            return true;
        }

        if (result != CreepWithdrawResult.Ok) {
            creep.LogInfo($"unexpected result when withdrawing from {target} ({result})");
        }

        return result == CreepWithdrawResult.Ok;
    }

    private static ISource? FindAssignedResource(this ICreep creep, RoomCache room) {
        var findNearest = room.FindNearestSource(creep.LocalPosition);
        if (findNearest == null) {
            return null;
        }

        if (creep.IsHarvester()) {
            // harvesters are allowed to use the closest source, no matter if main or not
            return findNearest;
        }

        if (room.Sources.Count < 1) {
            // if there is no other source, we have no chance
            return findNearest;
        }

        // all else should use other sources than the main for now
        if (!creep.Room!.Memory.TryGetString(RoomMainSource, out var mainSourceId)) return null;
        return room.Sources
            .Where(s => s.Id != mainSourceId)
            .FindNearest(creep.LocalPosition);
    }

    internal static void MoveToTransferIntoStorage(this ICreep creep, RoomCache room) {
        var spawn = room.FindNearestSpawn(creep.LocalPosition);
        if (spawn == null || spawn.Store.GetFreeCapacity(ResourceType.Energy) == 0) {
            creep.MoveToTransferIntoExtension(room);
            return;
        }

        var transferResult = creep.Transfer(spawn, ResourceType.Energy);
        if (transferResult == CreepTransferResult.NotInRange) {
            creep.BetterMoveTo(spawn.RoomPosition);
        } else if (transferResult != CreepTransferResult.Ok) {
            creep.LogInfo($"unexpected result when depositing to {spawn} ({transferResult})");
        }
    }

    internal static void MoveToTransferIntoExtension(this ICreep creep, RoomCache room) {
        var extension = room.Room.Find<IStructureExtension>()
            .Where(e => e.Store.GetFreeCapacity(ResourceType.Energy) > 0)
            .FindNearest(creep.LocalPosition);
        if (extension == null) {
            return;
        }

        var transferResult = creep.Transfer(extension, ResourceType.Energy);
        if (transferResult == CreepTransferResult.NotInRange) {
            creep.BetterMoveTo(extension.RoomPosition);
        } else if (transferResult != CreepTransferResult.Ok) {
            creep.LogInfo($"unexpected result when depositing to {extension} ({transferResult})");
        }
    }

    internal static bool IsHarvester(this ICreep creep) {
        return creep.GetJobId() == Harvester.JobId;
    }

    internal static string? GetJobId(this ICreep creep) {
        // First, see if we've stored the job instance on the creep from a previous tick (this will save us some CPU)
        if (creep.TryGetUserData<IJob>(out var job)) {
            return job.Id;
        }

        // Lookup their job from memory
        if (!creep.Memory.TryGetString(CreepJob, out var jobId)) {
            return null;
        }

        return jobId;
    }

    internal static CreepMoveResult BetterMoveTo(this ICreep creep, RoomPosition position) {
        return creep.MoveTo(
            position,
            new MoveToOptions(VisualizePathStyle: CreepManager.PathStyle)
        );
    }

    public static bool MoveToAttackInSameRoom(this ICreep creep) {
        // first priority: if we already have a target, follow it
        if (creep.Memory.TryGetString(CreepTarget, out var targetId) && !string.IsNullOrEmpty(targetId)) {
            var targetEnemy = creep.Room!.Find<ICreep>(my: false).SingleOrDefault(c => c.Id == targetId);
            if (creep.MoveToAttack(targetEnemy)) {
                return true;
            }
        }

        // second priority: attack foes!
        var enemy = creep.Room!.Find<ICreep>(my: false).FindNearest(creep.LocalPosition);
        if (enemy != null && creep.MoveToAttack(enemy)) {
            return true;
        }

        return false;
    }

    public static bool MoveToAttack(this ICreep creep, ICreep? enemy) {
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
            var jobId = creep.GetJobId();
            if (jobId != null) {
                creep.Room!.Memory.IncrementKillCount(jobId);
                creep.Memory.IncrementKillCount(jobId);
            }
        }

        return true;
    }

    public static bool MoveToBuild(this ICreep creep, IConstructionSite target) {
        var result = creep.Build(target);
        if (result == CreepBuildResult.NotInRange) {
            creep.BetterMoveTo(target.RoomPosition);
            return true;
        }

        if (result != CreepBuildResult.Ok) {
            creep.LogInfo($"unexpected result when depositing to {target} ({result})");
        }

        return result == CreepBuildResult.Ok;
    }
    
    public static void MoveAsUpgrader(this ICreep creep, RoomCache room)
    {
        // Check energy storage
        if (creep.Store[ResourceType.Energy] > 0) {
            // There is energy to use - upgrade the controller
            var controller = room.Room.Controller;
            if (controller == null) {
                return;
            }
            var upgradeResult = creep.UpgradeController(controller);
            if (upgradeResult == CreepUpgradeControllerResult.NotInRange) {
                creep.BetterMoveTo(controller.RoomPosition);
            } else if (upgradeResult != CreepUpgradeControllerResult.Ok) {
                creep.Say("⚠");
                creep.LogInfo($"unexpected result when depositing to {controller} ({upgradeResult})");
            }
        } else {
            // We're empty - go to pick up
            var spawn = room.FindNearestSpawn(creep.LocalPosition);
            if (spawn == null || spawn.Store.GetUsedCapacity() < 10) { // TODO: magic number
                creep.MoveToHarvestInRoom(room);
                return; 
            }
            creep.MoveToWithdraw(spawn);
        }
    }
}
