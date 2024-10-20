using System;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

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
        if (!game.Memory.TryGetObject("creeps", out var memoryCreeps)) {
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

    public static bool GetConfigBool(this IMemoryObject memory, string id) {
        return memory.GetOrCreateObject("config").TryGetBool(id, out var result) && result;
    }
}