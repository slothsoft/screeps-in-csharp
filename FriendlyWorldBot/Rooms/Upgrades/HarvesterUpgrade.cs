using System;
using System.Linq;
using FriendlyWorldBot.Rooms.Creeps;
using FriendlyWorldBot.Rooms.Structures;
using FriendlyWorldBot.Utils;
using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Rooms.Upgrades;

/// <summary>
/// This upgrade is supposed to switch Miner and Couriers back to Harvesters.
/// </summary>
public class HarvesterUpgrade : IUpgrade {
    public const string UpgradeId = nameof(HarvesterUpgrade);
    
    private readonly RoomCache _room;

    public HarvesterUpgrade(RoomCache room) {
        _room = room;
    }

    public string Id => UpgradeId;
    public bool ShouldBeStarted() => !_room.FindOfType<IStructureContainer>(StructureTypes.SourceContainer).Any();
    
    public UpgradeStatus Run() {
        // increase the number of harvesters to start building everything again
        _room.Room.SetWantedCreepsPerJob(Harvester.JobId, Harvester.DefaultCreepCount);
        // remvoe the miner upgrade so we can do it later again
        _room.Room.Memory.GetOrCreateObject(IMemoryConstants.RoomUpgrades).SetUpgradeStatus(MinerCourierUpgrade.UpgradeId, UpgradeStatus.NotStartedYet);
        return UpgradeStatus.Done;
    }
}