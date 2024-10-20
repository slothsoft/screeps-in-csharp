using System.Collections.Generic;
using System.Linq;
using ScreepsDotNet.API.Bot;
using ScreepsDotNet.API.World;
using FriendlyWorldBot.Rooms;
using FriendlyWorldBot.Utils;

namespace FriendlyWorldBot;

public class WorldBot : IBot {
    private readonly IGame _game;
    private readonly Dictionary<IRoom, RoomManager> _roomManagers = [];

    public WorldBot(IGame game) {
        _game = game;
        
        // Clean memory once on startup
        // (Since our IVM will reset periodically, this will run frequently enough without us needing to schedule it properly)
        _game.CleanMemory();
    }

    public void Loop() {
        // Check for any rooms that are no longer visible and remove their manager
        var removedRooms = _roomManagers.Keys.ToArray().Where(r => !r.Exists).Count(r => _roomManagers.Remove(r));
        if (removedRooms > 0) {
            Logger.Instance.Debug($"removed room manager for {removedRooms} rooms as they are no longer visible");
        }

        // Iterate over all visible rooms, create their manager if needed, and tick them
        foreach (var room in _game.Rooms.Values) {
            if (!room.Controller?.My ?? false) {
                continue;
            }

            if (!_roomManagers.TryGetValue(room, out var roomManager)) {
                Logger.Instance.Debug($"Adding room manager for {room} as it is now visible and controlled by us");
                roomManager = new RoomManager(_game, room);
                _roomManagers.Add(room, roomManager);
            }

            roomManager.Tick();
        }
    }
}