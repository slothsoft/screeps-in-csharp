﻿using FriendlyWorldBot.Gui;
using FriendlyWorldBot.Rooms.Creeps;
using FriendlyWorldBot.Rooms.Structures;
using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Rooms;

/// <summary>
/// The room manager will take care of all creep and spawning logic for a certain room controlled by our bot.
/// </summary>
public class RoomManager : IManager {
    private readonly RoomCache _cache;
    private readonly IManager[] _delegates;

    public RoomManager(IGame game, IRoom room) {
        _cache = new RoomCache(room);
        _delegates = [
            new StructureManager(game, _cache),
            new CreepManager(game, _cache),
            new GuiManager(game, _cache),
        ];
    }

    public void Tick() {
        foreach (var delegateManager in _delegates) {
            delegateManager.Tick();
        }
    }
}