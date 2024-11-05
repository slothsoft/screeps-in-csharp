using System.Resources;
using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Rooms.Structures;

/// <summary>
/// A storage type is a game object where creeps can add a resource it and / or retrieve it.
/// </summary>
public interface IStorageType {
    IRoomObject? FindBestInRoom(ICreep storageWorker, RoomCache room);
    
    bool CanTakeOut(ResourceType resourceType);
    int TakeOut(ICreep storageWorker, IRoomObject target, ResourceType resourceType);
    
    bool CanStore(ResourceType resourceType);
    int Store(ICreep storageWorker, IRoomObject target, ResourceType resourceType);
}