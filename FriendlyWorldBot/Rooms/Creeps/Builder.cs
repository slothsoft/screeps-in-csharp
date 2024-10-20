using System;
using System.Collections.Generic;
using System.Linq;
using FriendlyWorldBot.Utils;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Rooms.Creeps;

/// <summary>
/// Builder will try to repair buildings or build new ones. If both are not present, they will act like a miner.
/// </summary>
public class Builder : IJob {
    private const string MemoryIsBuilding = "isBuilding";

    private readonly RoomCache _room;

    public Builder(RoomCache room) {
        _room = room;
    }

    public string Id { get; } = "builder";
    public string Icon { get; } = "🛠";

    public void Run(ICreep creep) {
        creep.Memory.TryGetBool(MemoryIsBuilding, out var isBuilding);

        if (isBuilding) {
            RunInBuildMode(creep);

            if (creep.Store.GetUsedCapacity(ResourceType.Energy) == 0) {
                // we cannot build any longer, so switch to miner mode
                creep.Memory.SetValue(MemoryIsBuilding, false);
            }
        } else {
            RunInMinerMode(creep);

            if (creep.Store.GetFreeCapacity(ResourceType.Energy) == 0) {
                // we cannot mine any longer, so switch to builder mode
                creep.Memory.SetValue(MemoryIsBuilding, true);
            }
        }
    }

    private void RunInBuildMode(ICreep creep) {
        var constructionSite = creep.Room!.Find<IConstructionSite>().FirstOrDefault();
        if (constructionSite != null) {
            var transferResult = creep.Build(constructionSite);
            if (transferResult == CreepBuildResult.NotInRange) {
                creep.BetterMoveTo(constructionSite.RoomPosition);
            } else if (transferResult != CreepBuildResult.Ok) {
                creep.LogInfo($"unexpected result when depositing to {constructionSite} ({transferResult})");
            }
        } else {
            // no construction sites, so the builder switches to miner to be useful
            creep.PutIntoStorage(_room);
        }
    }

    private void RunInMinerMode(ICreep creep) {
        creep.MineResource(_room);
    }
}