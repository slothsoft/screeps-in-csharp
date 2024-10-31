using System;
using System.Collections.Generic;
using System.Linq;
using FriendlyWorldBot.Utils;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;
using static FriendlyWorldBot.Utils.IMemoryConstants;

namespace FriendlyWorldBot.Rooms.Structures;

/// <summary>
/// The structure struct has some additional functionality fot the IStructureType.
/// </summary>
public static class StructureTypes {
    internal static IList<IStructureType>? _all;
    public static IEnumerable<IStructureType> All => _all!;
   
    public static readonly StructureType<IStructureTower> Tower = new("towers", s =>{
        Logger.Instance.Error("Cannot create towers automatically");
        return new Position(0, 0);
    });
    
    // these are all kind of containers
    public static readonly StructureType<IStructureContainer> Container = new(MemoryContainers, room => room.FindNextSpawnLinePosition());
    public static readonly StructureType<IStructureContainer> Graveyard = new(MemoryContainers, room => room.FindNextSpawnLinePosition(), ContainerKindGraveyard);
    public static readonly StructureType<IStructureContainer> Source = new(MemoryContainers, room => room.FindNextSourceContainer(), ContainerKindSource);
}

public struct StructureType<TStructure> : IStructureType
    where TStructure : class, IStructure 
{
    
    private readonly string _collectionName;
    private readonly Func<RoomCache, Position?> _nextPosition;
    private readonly string? _expectedKind;

    public StructureType(string collectionName, Func<RoomCache, Position?> nextPosition, string? expectedKind = null) {
        _collectionName = collectionName;
        _nextPosition = nextPosition;
        _expectedKind = expectedKind;
        
        StructureTypes._all = new List<IStructureType>();
        StructureTypes._all.Add(this);
    }

    public string CollectionName => _collectionName;
    public string? Kind => _expectedKind;

    public bool IsAssignableFrom(IStructure structure) {
        if (structure is not TStructure) {
            return false;
        }
        if (_expectedKind != null) {
            if (structure.GetMemory().TryGetString(ContainerKind, out var actualKind)) {
                return actualKind == _expectedKind;
            }
            return false;
        }
        return true;
    }
    
    // TODO: maybe these methods would be more comfortable as IRoom or RoomCache extensions?
    
    public RoomCreateConstructionSiteResult CreateConstructionSite(IRoom room, Position position)
    {
        if (_expectedKind == null) {
            return room.CreateConstructionSite<TStructure>(position);
        }
        var futureMemory = new Dictionary<string, string> {
            { ContainerKind, _expectedKind },
        };
        return room.CreateConstructionSite<TStructure>(position, futureMemory);
    }
    
    public IEnumerable<TStructure> FindAll(RoomCache room) {
        return room.AllStructures.Where(IsAssignableFrom).OfType<TStructure>();
    }
    
    public (TStructure?, IConstructionSite?) FindOrCreateConstructionSite(RoomCache room) {
        // find an existing structure
        var resultStructure = FindAll(room).FirstOrDefault();
        if (resultStructure != null) {
            return (resultStructure, null);
        }
        // find a good place for placement
        var position = _nextPosition(room);
        if (position == null) return (null, null);
        
        // create a new structure
        var resultConstruction = CreateConstructionSite(room.Room, position.Value);
        if (resultConstruction == RoomCreateConstructionSiteResult.Ok) {
            var constructionSite = room.Room.LookForAt<IConstructionSite>(position.Value).FirstOrDefault();
            return (null, constructionSite);
        }
        return (null, null);
    }
}