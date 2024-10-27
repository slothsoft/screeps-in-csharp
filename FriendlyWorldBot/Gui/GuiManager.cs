using System;
using System.Collections.Generic;
using System.Linq;
using FriendlyWorldBot.Rooms;
using FriendlyWorldBot.Rooms.Creeps;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;
using static FriendlyWorldBot.Gui.IMenuConstants;

namespace FriendlyWorldBot.Gui;

public class GuiManager : IManager {
    
    private const int RoomWidth = 50;
    private const int RoomHeight = 50;

    private static readonly Color ColorYellow = new(byte.MaxValue, byte.MaxValue, 0);
    
    private static readonly RectVisualStyle MenuBarBackground = new(Fill: Color.Black, Opacity: 0.5);
    private static readonly TextVisualStyle MenuBarText = new(Stroke: Color.White, StrokeWidth: 0.01,Align: TextAlign.Left, Color: new Color(byte.MaxValue, byte.MaxValue, 0));
    private static readonly LineVisualStyle MenuBarLine = new(Color: ColorYellow);
    private const double MenuBarHeight = 1.4;
    private const double MenuPointMaxWidth = RoomWidth / 11.0;
    
    private readonly IGame _game;
    private readonly RoomCache _room;
    private readonly CreepManager _creepManager;

    public GuiManager(IGame game, RoomCache room, CreepManager creepManager) {
        _game = game;
        _room = room;
        _creepManager = creepManager;
    }

    public void Tick()
    {
       var menuBarHalfHeight = MenuBarHeight / 2; // 
        _room.Room.Visual.Rect(new FractionalPosition(-0.5, - menuBarHalfHeight), RoomWidth,MenuBarHeight, MenuBarBackground);
        var menuBarLineY = menuBarHalfHeight - (1.5 * MenuBarLine.Width ?? 0.1);
        _room.Room.Visual.Line(new FractionalPosition(-0.5, menuBarLineY), new FractionalPosition(RoomWidth, menuBarLineY), MenuBarLine);

        var menuPoints = new List<KeyValuePair<string, string>> {
            new(EnergyShort, _room.Room.EnergyAvailable + Of + _room.Room.EnergyCapacityAvailable)
        };
        foreach (var jobId in _creepManager.JobIds.OrderBy(jobId => _creepManager.GetJob(jobId).Priority)) {
            var job = _creepManager.GetJob(jobId);
            var wantedCreepCount = _creepManager.GetWantedCreepCount(job);
            if (wantedCreepCount > 0)
            {
                menuPoints.Add(new KeyValuePair<string, string>(job.Icon, _creepManager.GetActualCreepCount(jobId) + Of + wantedCreepCount));
            }
        }

        var pointWidth = Math.Min((double)RoomWidth / menuPoints.Count, MenuPointMaxWidth);
        var pointPadding = 0.25;
        var menuBarTextRightAlign = MenuBarText with
        {
            Align = TextAlign.Right,
        };
        var currentX = 0.0;
        foreach (var menuPoint in menuPoints) {
            _room.Room.Visual.Text(menuPoint.Key,  new FractionalPosition(currentX, pointPadding), MenuBarText);
            _room.Room.Visual.Text(menuPoint.Value,  new FractionalPosition(currentX + pointWidth - pointPadding, pointPadding), menuBarTextRightAlign);
            _room.Room.Visual.Line(new FractionalPosition(currentX + pointWidth, -0.5 + pointPadding / 2), new FractionalPosition(currentX + pointWidth, -0.5 + MenuBarHeight - pointPadding * 2), MenuBarLine);
            currentX += pointWidth;
        }
    }
}