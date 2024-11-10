using System;
using FriendlyWorldBot.Rooms.Creeps;
using FriendlyWorldBot.Rooms.Structures;
using FriendlyWorldBot.Utils;
using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Rooms.Upgrades;

/// <summary>
/// This upgrade is supposed to switch Harvesters to Miner and Couriers;
/// </summary>
public class MinerCourierUpgrade : IUpgrade {

    private const int ReducedHarvesters = 3;
    
    private readonly IGame _game;
    private readonly RoomCache _room;
    private readonly CreepManager _creepManager;

    public MinerCourierUpgrade(IGame game, RoomCache room, CreepManager creepManager) {
        _game = game;
        _room = room;
        _creepManager = creepManager;
    }

    public string Id => nameof(MinerCourierUpgrade);

    public bool ShouldBeStarted() => _room.Sources.Count == 1;
    
    public UpgradeStatus Run() {
        // Phase 1: reduce the number of harvesters
        var actualHarvesterCount = _room.Room.GetWantedCreepsPerJob(_creepManager.GetJob(Harvester.JobId));
        if (actualHarvesterCount > ReducedHarvesters) _room.Room.SetWantedCreepsPerJob(Harvester.JobId, ReducedHarvesters);
        
        // Phase 2: make sure we have the source container before we start constructing miners
        var (container, constructionSite) = _room.FindOrCreateConstructionSite(StructureTypes.SourceContainer);
        if (container == null) {
            Console.WriteLine("FOUND SOURCE CONTAINER!");
        }

        if (constructionSite != null) {
            Console.WriteLine("FOUND SOURCE CONTAINER CONSTRUCTION SITE!");
        }
        
        return UpgradeStatus.InProgress;
    }
}