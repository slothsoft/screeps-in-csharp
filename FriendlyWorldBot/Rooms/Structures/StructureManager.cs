using System;
using System.Collections.Generic;
using System.Linq;
using FriendlyWorldBot.Utils;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Rooms.Structures;

public class StructureManager : IManager {

    public static string MemoryMain = "main";
    public static string MemoryMainSource = "mainSource";
    
    private readonly IGame _game;
    private readonly RoomCache _room;

    public StructureManager(IGame game, RoomCache room) {
        _game = game;
        _room = room;

        var mainSpawn = room.Spawns.First();
        foreach (var spawn in room.Spawns) {
            spawn.Memory.SetValue(MemoryMain, spawn == mainSpawn);
        }
        var mainSource = room.Sources.FindNearest(mainSpawn.LocalPosition);
        foreach (var source in room.Sources) {
            source.Room!.Memory.SetValue(MemoryMainSource, source.Id);
        }
        Logger.Instance.Info($"set {mainSpawn} as main spawn and {mainSource} as main source");
    }

    public void Tick() {
        var controller = _room.Room.Controller;
        if (controller == null) return;
        var possibleExtensions = GetPossibleExtensionCount(controller.Level);
        var existingExtensions = _room.Extensions.Count;
        var showExtensions = _game.Memory.GetConfigBool("showExtensions");

        if (possibleExtensions > existingExtensions || showExtensions) {
            var positions = CreateExtensionPositions(_room.Spawns, possibleExtensions).ToList();
            
            if (showExtensions) {
                foreach (var fractionalPosition in positions) {
                    _room.Room.Visual.Circle(fractionalPosition, new CircleVisualStyle(
                        Radius: 0.5,
                        Stroke: Color.White,
                        StrokeWidth: 0.05));
                }
            }
        }
    }

    public static int GetPossibleExtensionCount(int roomLevel) {
        var sum = 0;
        for (var l = 0; l <= roomLevel; l++) {
            sum += l switch {
                0 => 0,
                1 or 2 => 5,
                _ => 10
            };
        }
        return sum;
    }

    public static IEnumerable<Position> CreateExtensionPositions(ICollection<IStructureSpawn> spawns, int expectedCount = 10) {
        IEnumerable<Position> result = new List<Position>();
        var expectedPerSpawn = expectedCount / spawns.Count;
        foreach (var spawn in spawns) {
            result = result.Concat(expectedPerSpawn.ToUlamSpiral(IsValidPosition).Select(pos => new Position(
                pos.X + spawn.LocalPosition.X,
                pos.Y + spawn.LocalPosition.Y
            )));
        }

        return result;
    }

    public static bool IsValidPosition(Position pos) {
        var absX = Math.Abs(pos.X);
        var absY = Math.Abs(pos.Y);
        if (absX < 2 && absY < 2) {
            // to close to the spawn
            return false;
        }

        if (absX == absY) {
            // these are the diagonal lines from the spawn
            return false;
        }

        if (absY < 2) {
            // the wide horizontal road
            return false;
        }

        if (absX == 0) {
            // the vertical road
            return false;
        }

        return true;
    }
}