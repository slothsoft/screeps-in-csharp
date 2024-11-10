using System;
using System.Collections.Generic;
using System.Linq;
using FriendlyWorldBot.Gui;
using FriendlyWorldBot.Paths;
using FriendlyWorldBot.Rooms.Structures;
using FriendlyWorldBot.Utils;
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
        var mainSpawn = roomCache.MainSpawn;
        if (mainSpawn == null) return null;
        
        foreach (var source in roomCache.Sources) {
            var closestPointOfSource = roomCache.GetClosestPointOfSource(mainSpawn, source);
            var closestPositionOfSource = new Position(closestPointOfSource.X, closestPointOfSource.Y);
            var pointIsEmpty = !roomCache.Room.LookAt(closestPositionOfSource).OfType<IStructure>().Any(s => s is not IStructureRoad);
            if (pointIsEmpty) {
                return closestPositionOfSource;
            }
        }

        return null;
    }
    
    public static Point GetClosestPointOfSource<TSource>(this RoomCache room, IStructureSpawn spawn, TSource source) 
        where TSource : IRoomObject, IWithId {
        // already in memory
        var memory = spawn.Memory.GetOrCreateObject(SpawnClosestPointOfSource);
        if (memory.TryGetString(source.Id, out var existingPoint)) return Point.Pathify(existingPoint);

        // find existing structures
        var sourceRectangle = new Rectangle(source.LocalPosition.X - 1, source.LocalPosition.Y - 1, source.LocalPosition.X + 1, source.LocalPosition.Y + 1);
        var (existingContainer, existingCs) = room.FindSingleStructureOrConstructionSite(
            StructureTypes.SourceContainer,
            o => sourceRectangle.Contains(o.LocalPosition.X, o.LocalPosition.Y));
        IRoomObject? existingRoomObject = existingContainer == null ? existingCs : existingContainer;
        if (existingRoomObject != null) {
            var containerPoint = new Point(existingRoomObject.LocalPosition.X, existingRoomObject.LocalPosition.Y);
            memory.SetValue(source.Id, containerPoint.Stringify());
            return containerPoint;
        }
        
        // we need to calculate
        var terrain = room.Room.GetTerrain();
        var closestPositionCount = sourceRectangle.ToPositions()
            .Where(p => terrain[p].IsTerrain(Terrain.Plain) || terrain[p].IsTerrain(Terrain.Swamp))
            .Where(p => !room.Room.LookAt(p).OfType<IStructure>().Any(s => s is not IStructureRoad))
            .Select(p => (p, room.Room.FindWalkingPath(spawn.LocalPosition, p).Count()))
            .MinBy(pc => pc.Item2);
        var closestPoint = new Point(closestPositionCount.p.X, closestPositionCount.p.Y);
        memory.SetValue(source.Id, closestPoint.Stringify());
        return closestPoint;
    }

    public static RoomCreateConstructionSiteResult CreateConstructionSite<TStructure>(this RoomCache roomCache, IStructureType structureType, Position position)
        where TStructure : class, IStructure 
    {
        if (structureType.Kind == null) {
            return roomCache.Room.CreateConstructionSite<TStructure>(position);
        }
        var futureMemory = new Dictionary<string, string> {
            { StructureKind, structureType.Kind },
        };
        return roomCache.Room.CreateConstructionSite<TStructure>(position, futureMemory);
    }
    
    private static IEnumerable<TStructure> FindOfType<TStructure>(this RoomCache room, IStructureType structureType) 
        where TStructure : class, IStructure 
    {
        return room.AllStructures.Where(structureType.IsAssignableFrom).OfType<TStructure>();
    }
    
    private static IEnumerable<IConstructionSite> FindConstructionSiteOfType<TStructure>(this RoomCache room, StructureType<TStructure> structureType) 
        where TStructure : class, IStructure 
    {
        return room.Room.Find<IConstructionSite>()
            .Where(c => c.IsStructure<TStructure>())
            .Where(c => structureType.IsMemoryAssignableFrom(c.GetFutureMemory()));
    }
    
    public static (TStructure?, IConstructionSite?) FindOrCreateConstructionSite<TStructure>(this RoomCache roomCache, AutoBuildStructureType<TStructure> structureType) 
        where TStructure : class, IStructure 
    {
        if (structureType.Kind == IMemoryConstants.ContainerKindSource) Console.WriteLine("FindOrCreateConstructionSite(" + structureType + ")");
        // find an existing structure
        var (resultStructure, resultConstruction) = roomCache.FindSingleStructureOrConstructionSite(structureType);
        if (resultStructure != null || resultConstruction != null) {
            if (structureType.Kind == IMemoryConstants.ContainerKindSource) Console.WriteLine("FOUND STRUCTURE");
            return (resultStructure, resultConstruction);
        }
        // find a good place for placement
        var position = structureType.FindNextPosition(roomCache);
        if (position == null) {
            if (structureType.Kind == IMemoryConstants.ContainerKindSource) Console.WriteLine("FOUND NO POINT");
            return (null, null);
        }
        
        // create a new structure
        var createConstruction = roomCache.CreateConstructionSite<TStructure>(structureType, position.Value);
        if (createConstruction == RoomCreateConstructionSiteResult.Ok) {
            var constructionSite = roomCache.Room.LookForAt<IConstructionSite>(position.Value).FirstOrDefault();
            if (structureType.Kind == IMemoryConstants.ContainerKindSource) Console.WriteLine("CREATE NEW CONSTRUCTION");
            return (null, constructionSite);
        }
        if (structureType.Kind == IMemoryConstants.ContainerKindSource) Console.WriteLine("FOUND NOTHING " + createConstruction);
        return (null, null);
    }
    
    public static (TStructure?, IConstructionSite?) FindSingleStructureOrConstructionSite<TStructure>(this RoomCache room, StructureType<TStructure> structureType, Func<IRoomObject, bool>? filter = null)
        where TStructure : class, IStructure {
        filter ??= _ => true;
        var resultStructure = room.FindOfType<TStructure>(structureType).FirstOrDefault(t => filter(t));
        var resultConstruction = resultStructure == null ? room.FindConstructionSiteOfType(structureType).FirstOrDefault(o => filter(o)) : null;
        return (resultStructure, resultConstruction);
    }
}