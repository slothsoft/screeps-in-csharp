using System.Linq;
using FriendlyWorldBot.Rooms.Structures;
using FriendlyWorldBot.Utils;
using ScreepsDotNet.API.World;
using static FriendlyWorldBot.Utils.IMemoryConstants;

namespace FriendlyWorldBot.Rooms.Creeps;

public static class CreepExtensions {
    internal static void MineResource(this ICreep creep, RoomCache room) {
        var source = creep.FindAssignedResource(room);
        if (source == null) {
            return;
        }

        var mineResult = creep.Harvest(source);
        if (mineResult == CreepHarvestResult.NotInRange) {
            creep.BetterMoveTo(source.RoomPosition);
        } else if (mineResult != CreepHarvestResult.Ok) {
            creep.LogInfo($"unexpected result when harvesting {source} ({mineResult})");
        }
    }

    private static ISource? FindAssignedResource(this ICreep creep, RoomCache room) {
        var findNearest = room.FindNearestSource(creep.LocalPosition);
        if (findNearest == null) {
            return null;
        }
        if (creep.IsMiner()) {
            // miners are allowed to use the closest source, no matter if main or not
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

    internal static void PutIntoStorage(this ICreep creep, RoomCache room) {
        var spawn = room.FindNearestSpawn(creep.LocalPosition);
        if (spawn == null || spawn.Store.GetFreeCapacity(ResourceType.Energy) == 0) {
            creep.PutIntoExtension(room);
            return;
        }

        var transferResult = creep.Transfer(spawn, ResourceType.Energy);
        if (transferResult == CreepTransferResult.NotInRange) {
            creep.BetterMoveTo(spawn.RoomPosition);
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
            creep.BetterMoveTo(extension.RoomPosition);
        } else if (transferResult != CreepTransferResult.Ok) {
            creep.LogInfo($"unexpected result when depositing to {extension} ({transferResult})");
        }
    }

    internal static bool IsMiner(this ICreep creep) {
        return creep.GetJobId() == Miner.JobId;
    }

    internal static string? GetJobId(this ICreep creep) {
        // First, see if we've stored the job instance on the creep from a previous tick (this will save us some CPU)
        if (creep.TryGetUserData<IJob>(out var job)) {
            return job.Id;
        }

        // Lookup their job from memory
        if (!creep.Memory.TryGetString("job", out var jobName)) {
            return null;
        }
      
        return jobName;
    }

    internal static CreepMoveResult BetterMoveTo(this ICreep creep, RoomPosition position) {
        return creep.MoveTo(
            position,
            new MoveToOptions(VisualizePathStyle: CreepManager.PathStyle)
        );
    }
}