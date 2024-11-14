using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FriendlyWorldBot.Rooms.Creeps;
using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Rooms.Structures;

public static class StorageTypes {

    internal static IList<IStorageType> _all = new List<IStorageType>();
    public static IEnumerable<IStorageType> All => _all.ToImmutableArray();
    
    public static readonly StructureTypeStorageType<IStructureSpawn> Spawn  = new(StructureTypes.Spawn);
    public static readonly StructureTypeStorageType<IStructureExtension> Extensions  = new(StructureTypes.Extension);
    public static readonly StructureTypeStorageType<IStructureTower> Tower = new(StructureTypes.Tower, canTakeOut: false);
    public static readonly StructureTypeStorageType<IStructureContainer> GraveyardContainer = new(StructureTypes.GraveyardContainer);
    public static readonly StructureTypeStorageType<IStructureContainer> SourceContainer = new(StructureTypes.SourceContainer);
    public static readonly StructureTypeStorageType<IStructureContainer> Container = new(StructureTypes.Container);
    
    public static readonly IStorageType Source = new SourceStorageType();
    
    
    public static readonly IStorageType[] DefaultTakeOutOrder = [GraveyardContainer, SourceContainer, Container, Extensions, Spawn, Source];
}

public struct SourceStorageType : IStorageType 
{
    public bool CanStore(ResourceType resourceType) => false;
    public bool Store(ICreep storageWorker, IRoomObject target, ResourceType resourceType) => false;
    public IRoomObject? FindBestToStoreInRoom(ICreep storageWorker, RoomCache room) => null;

    public bool CanTakeOut(ResourceType resourceType) => true;

    public IRoomObject? FindBestToTakeOutInRoom(ICreep storageWorker, RoomCache room) {
        return storageWorker.FindAssignedResource(room);
    }
    
    public bool TakeOut(ICreep storageWorker, IRoomObject target, ResourceType resourceType) {
        return storageWorker.MoveToHarvest((ISource) target);
    }
    
    public override string ToString() => "SourceStorageType";
}

public struct StructureStorageType<TStructure> : IStorageType 
    where TStructure : IStructure
{
    public StructureStorageType() {
    }

    public IRoomObject? FindBestToStoreInRoom(ICreep storageWorker, RoomCache room) {
        return room.Structures<TStructure>().FindNearest(storageWorker.LocalPosition);
    }

    public IRoomObject? FindBestToTakeOutInRoom(ICreep storageWorker, RoomCache room) {
        return room.Structures<TStructure>().FindNearest(storageWorker.LocalPosition);
    }

    public bool CanTakeOut(ResourceType resourceType) => true;

    public bool TakeOut(ICreep storageWorker, IRoomObject target, ResourceType resourceType) {
        return storageWorker.MoveToWithdraw((TStructure)target, resourceType);
    }

    public bool CanStore(ResourceType resourceType) => true;

    public bool Store(ICreep storageWorker, IRoomObject target, ResourceType resourceType) {
        throw new System.NotImplementedException();
    }
    
    public override string ToString() => typeof(TStructure).Name;
}

public struct StructureTypeStorageType<TStructure>(StructureType<TStructure> structureType, bool canTakeOut = true, bool canStore = true) : IStorageType 
    where TStructure : class, IStructure, IWithStore
{
    public IRoomObject? FindBestToStoreInRoom(ICreep storageWorker, RoomCache room) {
        var type = structureType;
        return room.Structures<TStructure>()
            .Where(s => type.IsAssignableFrom(s))
            .Where(s => s.Store.GetFreeCapacity(ResourceType.Energy) > 0)
            .FindNearest(storageWorker.LocalPosition);
    }
    
    public IRoomObject? FindBestToTakeOutInRoom(ICreep storageWorker, RoomCache room) {
        var type = structureType;
        return room.Structures<TStructure>()
            .Where(s => type.IsAssignableFrom(s))
            .Where(s => s.Store.GetUsedCapacity(ResourceType.Energy) > 0)
            .FindNearest(storageWorker.LocalPosition);
    }

    public bool CanTakeOut(ResourceType resourceType) => canTakeOut;

    public bool TakeOut(ICreep storageWorker, IRoomObject target, ResourceType resourceType) {
        return storageWorker.MoveToWithdraw((TStructure)target, resourceType);
    }

    public bool CanStore(ResourceType resourceType) => canStore;

    public bool Store(ICreep storageWorker, IRoomObject target, ResourceType resourceType) {
        return storageWorker.MoveToTransferInto((TStructure)target);
    }

    public override string ToString() => "StorageType." + typeof(TStructure).Name;
}