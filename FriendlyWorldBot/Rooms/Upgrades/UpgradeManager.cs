using System;
using System.Collections.Generic;
using FriendlyWorldBot.Rooms.Creeps;
using FriendlyWorldBot.Utils;
using ScreepsDotNet.API.World;
using static FriendlyWorldBot.Utils.IMemoryConstants;

namespace FriendlyWorldBot.Rooms.Upgrades;

public class UpgradeManager : IManager {
    private const int BuildEveryTicks = 60;

    private readonly IGame _game;
    private readonly RoomCache _room;
    private readonly IList<IUpgrade> _upgrades;

    public UpgradeManager(IGame game, RoomCache room, CreepManager creepManager) {
        _game = game;
        _room = room;
        _upgrades = new List<IUpgrade> {
            new MinerCourierUpgrade(game, room, creepManager),
        };
    }

    public void Tick() {
        if ((_game.Time + 30) % BuildEveryTicks != 0) return;

        var memory = _room.Room.Memory.GetOrCreateObject(RoomUpgrades);
        foreach (var upgrade in _upgrades) {
            var status = memory.TryGetUpgradeStatus(upgrade.Id);
            switch (status) {
                case UpgradeStatus.NotStartedYet: 
                    if (upgrade.ShouldBeStarted()) {
                        // start upgrade and update memory
                        memory.SetValue(upgrade.Id, upgrade.Run());
                    }
                    break;
                case UpgradeStatus.InProgress:
                    // continue upgrade 
                    memory.SetValue(upgrade.Id, upgrade.Run());
                    break;
                case UpgradeStatus.Done:
                default: // UpgradeStatus.Done
                    // we can ignore this status outright
                    break;
            }
        }
    }
}