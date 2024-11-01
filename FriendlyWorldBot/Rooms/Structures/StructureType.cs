using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using FriendlyWorldBot.Utils;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;
using static FriendlyWorldBot.Utils.IMemoryConstants;

namespace FriendlyWorldBot.Rooms.Structures;

/// <summary>
/// The structure struct has some additional functionality fot the IStructureType.
/// </summary>
public static class StructureTypes {
    internal static IList<IStructureType> _all = new List<IStructureType>();
    public static IEnumerable<IStructureType> All => _all.ToImmutableArray();
   
    public static readonly StructureType<IStructureTower> Tower = new("towers", s =>{
        Logger.Instance.Error("Cannot create towers automatically");
        return new Position(0, 0);
    });
    
    // these are all kind of containers
    public static readonly StructureType<IStructureContainer> Container = new(MemoryContainers, room => room.FindNextSpawnLinePosition());
    public static readonly StructureType<IStructureContainer> Graveyard = new(MemoryContainers, room => room.FindNextSpawnLinePosition(), ContainerKindGraveyard);
    public static readonly StructureType<IStructureContainer> Source = new(MemoryContainers, room => room.FindNextSourceContainerPosition(), ContainerKindSource);
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
        
        StructureTypes._all.Add(this);
    }

    public string CollectionName => _collectionName;
    public string? Kind => _expectedKind;

    public bool IsAssignableFrom(IStructure structure) {
        if (structure is not TStructure) {
            return false;
        }
        if (_expectedKind != null) {
            if (structure.GetMemory(CollectionName).TryGetString(ContainerKind, out var actualKind)) {
                return actualKind == _expectedKind;
            }
            return false;
        }
        return true;
    }

    public Position? FindNextPosition(RoomCache room) => _nextPosition(room);
}