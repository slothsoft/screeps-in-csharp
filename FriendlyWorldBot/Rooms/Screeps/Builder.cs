using System;
using System.Collections.Generic;
using System.Linq;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Rooms.Screeps;

/// <summary>
/// The harvester role will instruct creeps to harvest the nearest source until they're full, then return to the nearest spawn and deposit energy.
/// Sources and spawns will be cached to the heap for efficiency.
/// </summary>
/// <param name="room"></param>
public class Builder(IRoom room) : IJob {
    private readonly IRoom _room = room;

    private readonly List<ISource> _cachedSources = [];
    private readonly List<IStructureSpawn> _cachedSpawns = [];

    public void Run(ICreep creep) {
        creep.Say("🚧 build");

        creep.Memory.TryGetBool("building", out var building);
        
        // Check energy storage
        if (creep.Store.GetFreeCapacity(ResourceType.Energy) > 0 && ! building) {
            // There is space for more energy - harvest the nearest source
            var source = FindNearestSource(creep.LocalPosition);
            if (source == null) {
                return;
            }

            var harvestResult = creep.Harvest(source);
            if (harvestResult == CreepHarvestResult.NotInRange) {
                creep.MoveTo(source.RoomPosition);
            } else if (harvestResult != CreepHarvestResult.Ok) {
                Console.WriteLine($"{this}: {creep} unexpected result when harvesting {source} ({harvestResult})");
            }
        } else {
            // We're full - drop off at the nearest spawn
            creep.Memory.SetValue("building", true);
            var spawn = creep.Room!.Find<IConstructionSite>().FirstOrDefault();
            if (spawn == null) {
                return;
            }

            var transferResult = creep.Build(spawn);
            if (transferResult == CreepBuildResult.NotInRange) {
                creep.MoveTo(spawn.RoomPosition);
            } else if (transferResult != CreepBuildResult.Ok) {
                Console.WriteLine($"{this}: {creep} unexpected result when depositing to {spawn} ({transferResult})");
            }
            if (creep.Store.GetUsedCapacity(ResourceType.Energy) == 0)
                creep.Memory.SetValue("building", false);
        }
    }

    private ISource? FindNearestSource(Position pos) {
        // If there are no sources in the cache, assume the cache has not yet been built, so populate it
        if (_cachedSources.Count == 0) {
            foreach (var source in _room.Find<ISource>()) {
                _cachedSources.Add(source);
            }
        }

        // Now find the one closest to the position
        return _cachedSources.MinBy(x => x.LocalPosition.LinearDistanceTo(pos));
    }

    private IStructureSpawn? FindNearestSpawn(Position pos) {
        // If there are no spawns in the cache, assume the cache has not yet been built, so populate it
        if (_cachedSpawns.Count == 0) {
            foreach (var spawn in _room.Find<IStructureSpawn>()) {
                _cachedSpawns.Add(spawn);
            }
        }

        // Now find the one closest to the position (don't forget to check that it wasn't destroyed - we might want to do some cleanup of the cache if this happens)
        return _cachedSpawns
            .Where(static x => x.Exists)
            .MinBy(x => x.LocalPosition.LinearDistanceTo(pos));
    }
}