using FriendlyWorldBot.Rooms;
using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Gui;

public class GuiManager : IManager {
    private readonly IGame _game;
    private readonly RoomCache _room;

    public GuiManager(IGame game, RoomCache room) {
        _game = game;
        _room = room;
    }

    public void Tick() {
    }
}