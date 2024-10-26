using System.Collections.Generic;
using System.Linq;
using FriendlyWorldBot.Utils;
using ScreepsDotNet.API.World;
using static FriendlyWorldBot.Utils.IMemoryConstants;

namespace FriendlyWorldBot.Rooms.Creeps;

/// <summary>
/// A guard lives inside a rampart and attacks enemies or heals my game objects.
/// </summary>
public class Guard : IJob {
    private readonly IGame _game;
    private readonly RoomCache _room;

    public Guard(IGame game, RoomCache room) {
        _game = game;
        _room = room;
    }

    public string Id => "guard";
    public string Icon => "🛡️";
    public int WantedCreepCount => 0; //_room.Ramparts.Count;
    public IEnumerable<BodyPartGroup> BodyPartGroups => IJob.DefaultBodyPartGroups;

    public void Run(ICreep creep)
    {
    }
}