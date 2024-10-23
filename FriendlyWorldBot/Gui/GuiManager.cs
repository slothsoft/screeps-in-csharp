using FriendlyWorldBot.Rooms;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Gui;

public class GuiManager : IManager {
    
    private const int RoomWidth = 50;
    private const int RoomHeight = 50;

    private static readonly Color ColorYellow = new Color(byte.MaxValue, byte.MaxValue, 0);
    private static readonly RectVisualStyle MenuBarBackground = new(Fill: Color.Black, Opacity: 0.5);
    private static readonly TextVisualStyle MenuBarText = new(Stroke: Color.White, StrokeWidth: 0.01,Align: TextAlign.Left, Color: new Color(byte.MaxValue, byte.MaxValue, 0));
    
    private readonly IGame _game;
    private readonly RoomCache _room;

    public GuiManager(IGame game, RoomCache room) {
        _game = game;
        _room = room;
    }

    public void Tick() {
        _room.Room.Visual.Rect(new FractionalPosition(-0.5, -0.5), RoomWidth,1, MenuBarBackground);
        _room.Room.Visual.Text("Energy: " + _room.Room.EnergyAvailable + "/" + _room.Room.EnergyCapacityAvailable,  new FractionalPosition(0, 0.1), MenuBarText);
        _room.Room.Visual.Line(new FractionalPosition(-0.5, 0.4), new FractionalPosition(RoomWidth, 0.4),new LineVisualStyle(Color: ColorYellow));
    }
}