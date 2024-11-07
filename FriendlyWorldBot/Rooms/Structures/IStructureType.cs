using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Rooms.Structures;

/// <summary>
/// The structure inteface has some basic functionality for a IStructure implementation
/// </summary>
public interface IStructureType {    
    string CollectionName { get; }
    string? Kind { get; }
    bool IsAssignableFrom(IStructure structure);
}

public interface IWithAutoBuild {
    Position? FindNextPosition(RoomCache room);
}