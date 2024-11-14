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
    public const string UpgradeId = nameof(MinerCourierUpgrade);
    private const int ReducedHarvesters = 3;
    
    private readonly IGame _game;
    private readonly RoomCache _room;
    private readonly CreepManager _creepManager;

    public MinerCourierUpgrade(IGame game, RoomCache room, CreepManager creepManager) {
        _game = game;
        _room = room;
        _creepManager = creepManager;
    }

    public string Id => UpgradeId;
    public bool ShouldBeStarted() => 
        // Sources => Controller Level to Start
        // 0 => -1 
        // 1 =>  1
        // 2 =>  3
        // 3 =>  5
        _room.Room.Controller?.Level >= _room.Sources.Count * 2 - 1;
    
    public UpgradeStatus Run() {
        // Phase 1: reduce the number of harvesters
        var actualHarvesterCount = _room.Room.GetWantedCreepsPerJob(_creepManager.GetJob(Harvester.JobId));
        if (actualHarvesterCount > ReducedHarvesters) _room.Room.SetWantedCreepsPerJob(Harvester.JobId, ReducedHarvesters);
        
        // Phase 2: make sure we have the source container before we start constructing miners
        var (container, constructionSite) = _room.FindOrCreateConstructionSite(StructureTypes.SourceContainer);
        if (container != null || constructionSite == null) {
            return UpgradeStatus.InProgress;
        }
        
        // Phase 3: We now have a container and assume the spawn is creating the miner; so remove harvesters completely
        _room.Room.SetWantedCreepsPerJob(Harvester.JobId, 0);
        _room.Room.Memory.GetOrCreateObject(IMemoryConstants.RoomUpgrades).SetUpgradeStatus(HarvesterUpgrade.UpgradeId, UpgradeStatus.NotStartedYet);
        return UpgradeStatus.Done;
    }
}