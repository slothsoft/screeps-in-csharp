using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Rooms.Structures;

public interface IPurpose {
    
    void Run(IStructureTower tower);
}