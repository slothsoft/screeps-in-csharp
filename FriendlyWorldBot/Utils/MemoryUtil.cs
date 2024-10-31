using System;
using FriendlyWorldBot.Paths;
using FriendlyWorldBot.Rooms.Creeps;
using FriendlyWorldBot.Rooms.Structures;
using ScreepsDotNet;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;
using static FriendlyWorldBot.Utils.IMemoryConstants;

namespace FriendlyWorldBot.Utils;

public static class MemoryUtil {
    private static readonly int[] LastCollectCount = new int[GC.MaxGeneration];
    private static int _ticksSinceLastGc = 0;

    public static void LogGcActivity() {
        var didGc = false;
        for (var i = 0; i < GC.MaxGeneration; ++i) {
            var collectCount = GC.CollectionCount(i);
            if (collectCount > LastCollectCount[i]) {
                LastCollectCount[i] = collectCount;
                Logger.Instance.Info($"Gen {i} GC happened this loop (now up to {LastCollectCount[i]} collections)");
                didGc = true;
            }
        }

        if (!didGc) {
            ++_ticksSinceLastGc;
        }
    }

    public static void CleanMemory(this IGame game) {
        if (!game.Memory.TryGetObject(CreepCollection, out var memoryCreeps)) {
            return;
        }

        // Delete all creeps in memory that no longer exist
        var clearedCount = 0;
        foreach (var creepName in memoryCreeps.Keys) {
            if (!game.Creeps.ContainsKey(creepName)) {
                memoryCreeps.ClearValue(creepName);
                clearedCount++;
            }
        }

        if (clearedCount > 0) {
            Logger.Instance.Debug($"Cleared {clearedCount} dead creeps from memory");
        }
    }

    public static void CheckHeap(this in HeapInfo heapInfo) {
        if (_ticksSinceLastGc < 10) {
            return;
        }

        var heapUsageFrac = (heapInfo.TotalHeapSize + heapInfo.ExternallyAllocatedSize) / (double) heapInfo.HeapSizeLimit;
        if (heapUsageFrac > 0.65) {
            Logger.Instance.Info($"Heap usage is high ({heapUsageFrac * 100.0:N}%), running GC...");
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Default, true);
            _ticksSinceLastGc = 0;
        } else if (heapUsageFrac > 0.85) {
            Logger.Instance.Info($"Heap usage is very high ({heapUsageFrac * 100.0:N}%), running aggressive GC...");
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true);
            _ticksSinceLastGc = 0;
        } else if (_ticksSinceLastGc > 100) {
            Logger.Instance.Info($"GC hasn't run in a while (heap usage at {heapUsageFrac * 100.0:N}%), running GC...");
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Default, true);
            _ticksSinceLastGc = 0;
        }
    }

    public static bool GetConfigBool(this IGame game, string id) {
        return game.Memory.GetConfigObj().TryGetBool(id, out var result) && result;
    }
    
    private static IMemoryObject GetConfigObj(this IMemoryObject memory) {
        return memory.GetOrCreateObject("config");
    }

    public static IPath GetManualBuildConfigPath(this IRoom room, string id) {
        return room.Memory.GetConfigObj().GetOrCreateObject("manualBuild").TryGetString(id, out var path) ? EmptyPath.Instance :  path!.Pathify();
    }
    
    public static int GetWantedCreepsPerJob(this IRoom room, IJob job) {
        return room.Memory.GetConfigObj().GetOrCreateObject("wantedCreepsPerJob").TryGetInt(job.Id, out var result) ? result : job.WantedCreepCount;
    }

    public static void IncrementKillCount(this IMemoryObject memory, string id) {
        var killCount = GetKillCount(memory);
        killCount.ChangeIntValue(id, n => n + 1);
        killCount.ChangeIntValue(TotalEnemiesKilled, n => n + 1);
    }
    
    public static void ChangeIntValue(this IMemoryObject memory, string id, Func<int, int> change) {
        var currentValue = memory.TryGetInt(id, out var idKillCount) ? idKillCount : 0;
        memory.SetValue(id, change(currentValue));
    }

    private static IMemoryObject GetKillCount(this IMemoryObject memory) {
        return memory.GetOrCreateObject("killCount");
    }
    
    public static IPath TryGetPath(this IMemoryObject memory, string id) {
        return memory.TryGetString(id, out var path) ? EmptyPath.Instance :  path!.Pathify();
    }
    
    public static void SetValue(this IMemoryObject memory, string id, IPath path) {
        memory.SetValue(id, path.Stringify());
    }
    
    public static IMemoryObject GetMemory(this IStructure structure) {
        foreach (var type in StructureType.All) {
            if (type.IsAssignableFrom(structure)) {
                return structure.GetMemory(type.CollectionName);
            }
        }
        
        Logger.Instance.Error("COULD NOT GET MEMORY OF STRUCTURE: " + structure);
        return Program.Game!.Memory;
    }

    public static IMemoryObject GetMemory<TObject>(this TObject obj, string collectionName) 
        where TObject : IWithId, IRoomObject
    {
        return Program.Game!.Memory.GetOrCreateObject(collectionName).GetOrCreateObject(obj.Id);
    }
}