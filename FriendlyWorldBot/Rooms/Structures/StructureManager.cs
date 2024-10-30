using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Rooms.Structures;

public class StructureManager : IManager {
    private readonly IGame _game;
    private readonly RoomCache _room;

    private readonly TowerPurpose _tower = new();
    
    public StructureManager(IGame game, RoomCache room) {
        _game = game;
        _room = room;
    }

    public void Tick() {
        foreach (var structureTower in _room.Towers) {
            _tower.Run(structureTower);
        }
    }
}