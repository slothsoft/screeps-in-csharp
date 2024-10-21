using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Rooms.Creeps;

/// <summary>
/// The miner job will instruct creeps to harvest the nearest source until they're full, then return to the nearest spawn and deposit energy.
/// Sources and spawns will be cached to the heap for efficiency, so if other jobs need this functionality, use this instance.
/// </summary>
public class Miner : IJob {
    internal const string JobId = "miner";
    private readonly RoomCache _room;

    public Miner(RoomCache room) {
        _room = room;
    }

    public string Id => JobId;
    public string Icon => "⛏";
    public int WantedCreepCount => 3;

    public void Run(ICreep creep) {
        // Check energy storage
        if (creep.Store.GetFreeCapacity(ResourceType.Energy) > 0) {
            creep.MineResource(_room);
        } else {
            creep.PutIntoStorage(_room);
        }
    }
}