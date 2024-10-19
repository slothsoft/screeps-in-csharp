using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Rooms.Creeps;

/// <summary>
/// Creeps are assigned a job at birth and have to do it
/// until they die.
/// </summary>
public interface IJob {
    string Id { get; }
    string Icon { get; }

    void Run(ICreep creep);
}