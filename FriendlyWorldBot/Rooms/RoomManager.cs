using System;
using System.Linq;
using FriendlyWorldBot.Gui;
using FriendlyWorldBot.Rooms.Creeps;
using FriendlyWorldBot.Rooms.Structures;
using ScreepsDotNet.API.World;
using static FriendlyWorldBot.Utils.IMemoryConstants;

namespace FriendlyWorldBot.Rooms;

/// <summary>
/// The room manager will take care of all creep and spawning logic for a certain room controlled by our bot.
/// </summary>
public class RoomManager : IManager {
    private readonly RoomCache _cache;
    private readonly IManager[] _delegates;

    public RoomManager(IGame game, IRoom room) {
        _cache = new RoomCache(room);

        if (!room.Memory.TryGetString(RoomName, out _))
        {
            var spawnName = _cache.Spawns.FirstOrDefault()?.Name ?? string.Empty;
            var spawnNameShort = spawnName.Length > 0 ? spawnName[..1] : string.Empty;
            room.Memory.SetValue(RoomName, spawnName);
            room.Memory.SetValue(RoomNameShort, spawnNameShort);
        }
        
        var creepManager = new CreepManager(game, _cache);
        _delegates = [
            new StructureManager(game, _cache),
            creepManager,
            new GuiManager(game, _cache, creepManager),
        ];
    }

    public void Tick() {
        foreach (var delegateManager in _delegates) {
            delegateManager.Tick();
        }
    }
}