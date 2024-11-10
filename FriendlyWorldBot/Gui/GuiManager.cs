using FriendlyWorldBot.Rooms;
using FriendlyWorldBot.Rooms.Creeps;
using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Gui;

public class GuiManager : IManager {
    private readonly IManager[] _visualizers;

    public GuiManager(IGame game, RoomCache room, CreepManager creepManager) {
        _visualizers = [
            new StructureInfoVisualizer(game, room),
            new MenuVisualizer(room, creepManager),
        ];
    }

    public void Tick() {
        foreach (var visualizer in _visualizers) {
            visualizer.Tick();
        }
    }
}