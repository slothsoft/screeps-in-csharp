using System.Collections.Generic;
using System.Linq;
using FriendlyWorldBot.Gui;
using FriendlyWorldBot.Rooms.Structures;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;
using static FriendlyWorldBot.Utils.IMemoryConstants;

namespace FriendlyWorldBot.Rooms;

public static class RoomCacheExtensions {
    
    public static Position? FindNextSpawnLinePosition(this RoomCache roomCache) {
        var spawnPos = roomCache.MainSpawn?.LocalPosition;
        if (spawnPos == null) return null;

        var terrain = roomCache.Room.GetTerrain();
        const int step = 2;
        var distance = step;
        do {
            var newPos = new Position(spawnPos.Value.X + distance, spawnPos.Value.Y);
            if (newPos.X < IGuiConstants.RoomWidth && roomCache.AllStructures.All(s => s.LocalPosition != newPos)) {
                if (!terrain[newPos].IsTerrain(Terrain.Wall)) {
                    return newPos;
                }
            }

            newPos = new Position(spawnPos.Value.X - distance, spawnPos.Value.Y);
            if (newPos.X >= 0 && roomCache.AllStructures.All(s => s.LocalPosition != newPos)) {
                if (!terrain[newPos].IsTerrain(Terrain.Wall)) {
                    return newPos;
                }
            }

            distance += step;
        } while (spawnPos.Value.X + distance < IGuiConstants.RoomWidth && spawnPos.Value.X - distance >= 0);

        return null;
    }

    public static Position? FindNextSourceContainerPosition(this RoomCache roomCache) {
        var spawnPos = roomCache.MainSpawn?.LocalPosition;
        if (spawnPos == null) return null;

        const int step = 2;
        var distance = step;
        do {
            var newPos = new Position(spawnPos.Value.X + distance, spawnPos.Value.Y);
            if (newPos.X < IGuiConstants.RoomWidth && roomCache.AllStructures.All(s => s.LocalPosition != newPos)) {
                return newPos;
            }

            newPos = new Position(spawnPos.Value.X - distance, spawnPos.Value.Y);
            if (newPos.X >= 0 && roomCache.AllStructures.All(s => s.LocalPosition != newPos)) {
                return newPos;
            }

            distance += step;
        } while (spawnPos.Value.X + distance < IGuiConstants.RoomWidth && spawnPos.Value.X - distance >= 0);

        return null;
    }
    
    public static RoomCreateConstructionSiteResult CreateConstructionSite<TStructure>(this RoomCache roomCache, IStructureType structureType, Position position)
        where TStructure : class, IStructure 
    {
        if (structureType.Kind == null) {
            return roomCache.Room.CreateConstructionSite<TStructure>(position);
        }
        var futureMemory = new Dictionary<string, string> {
            { ContainerKind, structureType.Kind },
        };
        return roomCache.Room.CreateConstructionSite<TStructure>(position, futureMemory);
    }
    
    private static IEnumerable<TStructure> FindAll<TStructure>(this RoomCache room, IStructureType structureType) 
        where TStructure : class, IStructure 
    {
        return room.AllStructures.Where(structureType.IsAssignableFrom).OfType<TStructure>();
    }
    
    public static (TStructure?, IConstructionSite?) FindOrCreateConstructionSite<TStructure>(this RoomCache roomCache, AutoBuildStructureType<TStructure> structureType) 
        where TStructure : class, IStructure 
    {
        // find an existing structure
        var resultStructure = roomCache.FindAll<TStructure>(structureType).FirstOrDefault();
        if (resultStructure != null) {
            return (resultStructure, null);
        }
        // find a good place for placement
        var position = structureType.FindNextPosition(roomCache);
        if (position == null) return (null, null);
        
        // create a new structure
        var resultConstruction = roomCache.CreateConstructionSite<TStructure>(structureType, position.Value);
        if (resultConstruction == RoomCreateConstructionSiteResult.Ok) {
            var constructionSite = roomCache.Room.LookForAt<IConstructionSite>(position.Value).FirstOrDefault();
            return (null, constructionSite);
        }
        return (null, null);
    }
}