using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
    internal static IList<IStructureType> _all = new List<IStructureType>();
    public static IEnumerable<IStructureType> All => _all.ToImmutableArray();

    public static IStructureType? FetchType(this IStructure structure) => All.FirstOrDefault(a => a.IsAssignableFrom(structure)); 
    
    public static readonly StructureType<IStructureSpawn> Spawn = new(MemorySpawns);
    public static readonly StructureType<IStructureExtension> Extension = new(IMemoryConstants.MemoryExtensions);

    public static readonly StructureType<IStructureTower> Tower = new(MemoryTowers);
    
    // these are all kind of containers
    public static readonly AutoBuildStructureType<IStructureContainer> Container = new(MemoryContainers, room => room.FindNextSpawnLinePosition());
    public static readonly AutoBuildStructureType<IStructureContainer> GraveyardContainer = new(MemoryContainers, room => room.FindNextSpawnLinePosition(), ContainerKindGraveyard);
    public static readonly AutoBuildStructureType<IStructureContainer> SourceContainer = new(MemoryContainers, room => room.FindNextSourceContainerPosition(), ContainerKindSource);
    
    public static IEnumerable<ResourceType> ContainedResourceTypes(this IStructure structure) {
        if (structure is IWithStore withStore) {
            foreach (var resourceType in Enum.GetValues<ResourceType>()) {
                if (withStore.Store[resourceType] > 0) yield return resourceType;
            }
        }
    }
}

public record AutoBuildStructureType<TStructure>(string CollectionName, Func<RoomCache, Position?> NextPosition, string? ExpectedKind = null) 
    : StructureType<TStructure>(CollectionName, ExpectedKind), IWithAutoBuild
    where TStructure : class, IStructure {
    public Position? FindNextPosition(RoomCache room) => NextPosition(room);
}

public record StructureType<TStructure> : IStructureType
    where TStructure : class, IStructure 
{
    
    private readonly string _collectionName;
    private readonly string? _expectedKind;

    public StructureType(string collectionName, string? expectedKind = null) {
        _collectionName = collectionName;
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
}