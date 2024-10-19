using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Rooms.Screeps;

/// <summary>
/// 
/// </summary>

public interface IJob
{
    void Run(ICreep creep);
}