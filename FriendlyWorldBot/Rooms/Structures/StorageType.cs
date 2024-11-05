using System.Collections.Generic;
using System.Collections.Immutable;
using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Rooms.Structures;

public static class StorageTypes {

    internal static IList<IStorageType> _all = new List<IStorageType>();
    public static IEnumerable<IStorageType> All => _all.ToImmutableArray();
    
    public static readonly StructureStorageType Spawn = new();
    public static readonly StructureStorageType Extensions = new();
    public static readonly StructureStorageType Tower = new();
    public static readonly StructureStorageType GraveyardContainer = new();
    public static readonly StructureStorageType SourceContainer = new();
}

public struct StructureStorageType<TStructure> : IStorageType 
    where TStructure : IStructure
{
    public StorageType() {
    }

    public IRoomObject? FindBestInRoom(ICreep storageWorker, RoomCache room) {
        return room.Structures<TStructure>().FindNearest(storageWorker.LocalPosition);
    }

    public bool CanTakeOut(ResourceType resourceType) => true;

    public int TakeOut(ICreep storageWorker, IRoomObject target, ResourceType resourceType) {
        return storageWorker.Withdraw((TStructure)target, resourceType);
    }

    public bool CanStore(ResourceType resourceType) => true;

    public int Store(ICreep storageWorker, IRoomObject target, ResourceType resourceType) {
        throw new System.NotImplementedException();
    }
}