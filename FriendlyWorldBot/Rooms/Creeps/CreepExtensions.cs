using System.Linq;
using FriendlyWorldBot.Rooms.Structures;
using FriendlyWorldBot.Utils;
using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Rooms.Creeps;

public static class CreepExtensions {
    internal static void MineResource(this ICreep creep, RoomCache room) {
        var source = room.FindNearestSource(creep.LocalPosition);
        if (source == null) {
            return;
        }

        var mineResult = creep.Harvest(source);
        if (mineResult == CreepHarvestResult.NotInRange) {
            creep.MoveTo(source.RoomPosition);
        } else if (mineResult != CreepHarvestResult.Ok) {
            creep.LogInfo($"unexpected result when harvesting {source} ({mineResult})");
        }
    }

    internal static void PutIntoStorage(this ICreep creep, RoomCache room) {
        var spawn = room.FindNearestSpawn(creep.LocalPosition);
        if (spawn == null || spawn.Store.GetFreeCapacity(ResourceType.Energy) > 0) {
            creep.PutIntoExtension(room);
            return;
        }

        var transferResult = creep.Transfer(spawn, ResourceType.Energy);
        if (transferResult == CreepTransferResult.NotInRange) {
            creep.MoveTo(spawn.RoomPosition);
        } else if (transferResult != CreepTransferResult.Ok) {
            creep.LogInfo($"unexpected result when depositing to {spawn} ({transferResult})");
        }
    }

    internal static void PutIntoExtension(this ICreep creep, RoomCache room) {
        var extension = room.Room.Find<IStructureExtension>().Where(e => e.Store.GetFreeCapacity(ResourceType.Energy) > 0).FindNearest(creep.LocalPosition);
        if (extension == null) {
            return;
        }

        var transferResult = creep.Transfer(extension, ResourceType.Energy);
        if (transferResult == CreepTransferResult.NotInRange) {
            creep.MoveTo(extension.RoomPosition);
        } else if (transferResult != CreepTransferResult.Ok) {
            creep.LogInfo($"unexpected result when depositing to {extension} ({transferResult})");
        }
    }
}