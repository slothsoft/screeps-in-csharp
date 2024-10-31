using System.Linq;
using FriendlyWorldBot.Gui;
using FriendlyWorldBot.Paths;
using FriendlyWorldBot.Rooms.Creeps;
using FriendlyWorldBot.Rooms.Structures;
using FriendlyWorldBot.Rooms.Upgrades;
using FriendlyWorldBot.Utils;
using ScreepsDotNet.API.World;
using static FriendlyWorldBot.Utils.IMemoryConstants;

namespace FriendlyWorldBot.Rooms;

/// <summary>
/// The room manager will take care of all creep and spawning logic for a certain room controlled by our bot.
/// </summary>
public class RoomManager : IManager {
    private readonly RoomCache _cache;
    private readonly IManager[] _delegates;

    public RoomManager(IGame game, IRoom room) {
        _cache = new RoomCache(room);
        var creepManager = new CreepManager(game, _cache);
        _delegates = [
            new StructureBuilder(game, _cache),
            creepManager,
            new GuiManager(game, _cache, creepManager),
            new StructureManager(game, _cache),
            new UpgradeManager(game, _cache, creepManager),
        ];
    }

    public void Tick() {
        _cache.Tick();
        
        // tick all the managers (off)
        foreach (var delegateManager in _delegates) {
            delegateManager.Tick();
        }

        // check if any of the structures for the future position was filled
        foreach (var futureMemoryPosition in _cache.Room.FetchFutureMemoryPositions()) {
            var buildStructure = _cache.AllStructures.FirstOrDefault(s => s.LocalPosition == futureMemoryPosition);
            if (buildStructure != null) {
                // fill the structure with the given memory
                var currentMemory = buildStructure.GetMemory();
                var futureMemory = _cache.Room.FetchFutureMemory(futureMemoryPosition);
                foreach (var futureMemoryKey in futureMemory.Keys) {
                    if (futureMemory.TryGetString(futureMemoryKey, out var value)) {
                        currentMemory.SetValue(futureMemoryKey, value);
                    }
                }
                // now remove the future memory once and for all
                _cache.Room.Memory.GetOrCreateObject(RoomFutureMemory).ClearValue(new Point(futureMemoryPosition.X, futureMemoryPosition.Y).Stringify());
            }         
        }
    }
}