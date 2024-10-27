using System.Collections.Generic;
using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Rooms.Creeps;

/// <summary>
/// The upgrader job will instruct creeps to collect energy from the spawn, then upgrade the room controller.
/// Spawns will be cached to the heap for efficiency.
/// </summary>
public class Upgrader : IJob
{
    private readonly RoomCache _room;
    
    public Upgrader(RoomCache room) {
        _room = room;
    }
    
    public string Id { get; } = "upgrader";
    public string Icon { get; } = "🗼";
    public int WantedCreepCount => 3;
    public IEnumerable<BodyPartGroup> BodyPartGroups => IJob.DefaultBodyPartGroups;
    public int Priority => 10;
    public void Run(ICreep creep) => creep.MoveAsUpgrader(_room);
}