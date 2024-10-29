using System;
using System.Linq;
using FriendlyWorldBot.Utils;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;
using static FriendlyWorldBot.Utils.IMemoryConstants;

namespace FriendlyWorldBot.Rooms.Structures;

public partial class StructureManager {
    private bool BuildRoads() {
        return BuildManualRoads() || BuildSpawnRoads() || BuildAutomaticRoads();
    }

    private bool BuildManualRoads()
    {
        // _game.Constants.Controller.GetMaxStructureCount<>()
        // _room.Room.GetManualBuildConfigPath()
        return false;
    }

    private bool BuildSpawnRoads() {
        var createdRoomsForLevel = _room.Room.Memory.TryGetInt(RoomCreatedRoadsForLevel, out var l) ? l : 0;
        var controllerLevel = _room.Room.Controller!.Level;
        // TODO: if (createdRoomsForLevel >= controllerLevel) return false;
        
        var roadCount = 0;
        var maxExtensions = _game.Constants.Controller.GetMaxStructureCount<IStructureExtension>(controllerLevel);
        var additionalExtensions = _room.Room.Memory.TryGetInt(RoomAdditionalExtensions, out var ae) ? ae : 0;
        var maxSpawnPositions =  (maxExtensions + additionalExtensions + 10).ToUlamSpiral().ToArray(); // TODO: magic number for the spaces between extensions

        if (maxSpawnPositions.Length == 0) {
            return false; // there is no spawn yet, so we cannot build anything
        }
        
        var minX = maxSpawnPositions.Select(p => p.X).Min();
        var minY = maxSpawnPositions.Select(p => p.Y).Min();
        var maxX = maxSpawnPositions.Select(p => p.X).Max();
        var maxY = maxSpawnPositions.Select(p => p.Y).Max();
        
        foreach (var spawn in _room.SpawnsForExtensionConstruction) {
            var spawnPosition = spawn.LocalPosition;
            var terrains = _room.Room.LookForAtArea<IStructureWall>(new Position(minX + spawnPosition.X, minY + spawnPosition.Y), new Position(maxX + spawnPosition.X, maxY + spawnPosition.Y)).ToList();

            for (var x = minX; x <= maxX; x++) {
                for (var y = minY; y <= maxY; y++) {
                    if (IsValidRoadPosition(x, y)) {
                        var roomX = x + spawnPosition.X;
                        var roomY = y + spawnPosition.Y;
                        if (!terrains.Any(t => t.LocalPosition.X == roomX && t.LocalPosition.Y == roomY))
                        {
                            var constructionResult = _room.Room.CreateConstructionSite<IStructureRoad>(new Position(roomX, roomY));
                            if (constructionResult == RoomCreateConstructionSiteResult.Ok) {
                                roadCount++;
                                if (roadCount >= MaxConstructionSites) {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
        }
        
        _room.Room.Memory.SetValue(RoomCreatedRoadsForLevel, controllerLevel);
        return roadCount > 0;
    }
    
    private static bool IsValidRoadPosition(int x, int y) {
        var absX = Math.Abs(x);
        var absY = Math.Abs(y);
        if (absY == 0) {
            // the horizontal not-road is speckled
            return absX % 2 == 1;
        }

        return !IsValidExtensionPosition(x, y);
    }

    private bool BuildAutomaticRoads()
    {
        return false;
    }
}