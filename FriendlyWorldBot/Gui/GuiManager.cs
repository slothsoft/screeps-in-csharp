using System.Collections.Generic;
using FriendlyWorldBot.Rooms;
using FriendlyWorldBot.Rooms.Creeps;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;
using static FriendlyWorldBot.Gui.IMenuConstants;

namespace FriendlyWorldBot.Gui;

public class GuiManager : IManager {
    
    private const int RoomWidth = 50;
    private const int RoomHeight = 50;

    private static readonly Color ColorYellow = new Color(byte.MaxValue, byte.MaxValue, 0);
    private static readonly RectVisualStyle MenuBarBackground = new(Fill: Color.Black, Opacity: 0.5);
    private static readonly TextVisualStyle MenuBarText = new(Stroke: Color.White, StrokeWidth: 0.01,Align: TextAlign.Left, Color: new Color(byte.MaxValue, byte.MaxValue, 0));
    
    private readonly IGame _game;
    private readonly RoomCache _room;
    private readonly CreepManager _creepManager;

    public GuiManager(IGame game, RoomCache room, CreepManager creepManager) {
        _game = game;
        _room = room;
        _creepManager = creepManager;
    }

    public void Tick() {
        _room.Room.Visual.Rect(new FractionalPosition(-0.5, -0.5), RoomWidth,1, MenuBarBackground);
        _room.Room.Visual.Line(new FractionalPosition(-0.5, 0.4), new FractionalPosition(RoomWidth, 0.4),new LineVisualStyle(Color: ColorYellow));

        var menuPoints = new Dictionary<string, string> {
            {EnergyShort, _room.Room.EnergyAvailable + Of + _room.Room.EnergyCapacityAvailable}
        };
        foreach (var jobId in _creepManager.JobIds) {
            var job = _creepManager.GetJob(jobId);
            menuPoints.Add(job.Icon, _creepManager.GetActualCreepCount(jobId) + Of + job.WantedCreepCount);
        }

        var pointWidth = (double) RoomWidth / menuPoints.Count;
        var currentX = 0.0;
        foreach (var menuPoint in menuPoints) {
            _room.Room.Visual.Text(menuPoint.Key + ": " + menuPoint.Value,  new FractionalPosition(0, 0.1), MenuBarText);
            currentX += pointWidth;
        }

    }
}