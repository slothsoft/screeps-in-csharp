using System.Resources;
using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Rooms.Structures;

/// <summary>
/// A storage type is a game object where creeps can add a resource it and / or retrieve it.
/// </summary>
public interface IStorageType {
    bool CanTakeOut(ResourceType resourceType);
    IRoomObject? FindBestToTakeOutInRoom(ICreep storageWorker, RoomCache room);
    bool TakeOut(ICreep storageWorker, IRoomObject target, ResourceType resourceType);
    
    bool CanStore(ResourceType resourceType);
    IRoomObject? FindBestToStoreInRoom(ICreep storageWorker, RoomCache room);
    bool Store(ICreep storageWorker, IRoomObject target, ResourceType resourceType);
}