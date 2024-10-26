using System;
using System.Collections.Generic;
using System.Linq;
using FriendlyWorldBot.Utils;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;
using static FriendlyWorldBot.Utils.IMemoryConstants;

namespace FriendlyWorldBot.Rooms.Structures;

public partial class StructureManager : IManager
{
    private const int BuildEveryTicks = 60;
    
    private readonly IGame _game;
    private readonly RoomCache _room;

    public StructureManager(IGame game, RoomCache room) {
        _game = game;
        _room = room;

        var mainSpawn = room.Spawns.First();
        foreach (var spawn in room.Spawns) {
            spawn.Memory.SetValue(SpawnMain, spawn == mainSpawn);
        }
        var mainSource = room.Sources.FindNearest(mainSpawn.LocalPosition);
        foreach (var source in room.Sources) {
            source.Room!.Memory.SetValue(RoomMainSource, source.Id);
        }
        Logger.Instance.Info($"set {mainSpawn} as main spawn and {mainSource} as main source");
    }

    public void Tick()
    {
        if (_game.Time % BuildEveryTicks != 0) return;
        var somethingWasBuild = BuildExtensions();
        if (!somethingWasBuild)
        {
            Logger.Instance.Info("Nothing needs to be build.");
        }
    }
}