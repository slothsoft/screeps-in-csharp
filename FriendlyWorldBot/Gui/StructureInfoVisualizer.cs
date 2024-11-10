using System.Linq;
using FriendlyWorldBot.Rooms;
using FriendlyWorldBot.Rooms.Structures;
using FriendlyWorldBot.Utils;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;
using static FriendlyWorldBot.Gui.IGuiConstants;
using static FriendlyWorldBot.Utils.IMemoryConstants;

namespace FriendlyWorldBot.Gui;

public class StructureInfoVisualizer : IManager {
    
    private const string Separator = ",";
    
    private const double InfoPadding = 0.2;
    private const double InfoLineHeight = 0.6;
    private const double InfoWidth = 6;
    
    private static readonly RectVisualStyle InfoBox = new(Fill: Color.Black, Opacity: 0.75, Stroke: ColorGolden, StrokeWidth: 0.1);
    private static readonly LineVisualStyle InfoSeparator = new(Color: ColorGolden);
    private static readonly TextVisualStyle LeftColumn = new(Color: Color.White, Stroke: Color.White, StrokeWidth: 0.005, Align: TextAlign.Left, Font: "0.4");
    private static readonly TextVisualStyle RightColumn = LeftColumn with { Align = TextAlign.Right};
    private static readonly TextVisualStyle Title = LeftColumn with { Color = ColorGolden, Stroke = ColorGolden, Font= "0.4", StrokeWidth = 0.01 };
    private static readonly CircleVisualStyle StructureMarker = new(Radius: InfoLineHeight/2, Fill: ColorTransparent, Opacity: 1, Stroke: Color.Red, StrokeWidth: 0.1);
    
    private readonly IGame _game;
    private readonly RoomCache _room;

    public StructureInfoVisualizer(IGame game, RoomCache room) {
        _game = game;
        _room = room;
    }

    public void Tick() {
        if (!_game.Memory.GetConfigObj().TryGetString(RoomDisplayStructureInfo, out var displayStructures)) return;

        var ids = displayStructures.Split(Separator);
        foreach (var id in ids) {
            var structure = _room.AllStructures.SingleOrDefault(st => st.Id.ToString().Equals(id));
            if (structure == null) continue;

            var storage = structure is IStructureStorage structureStorage ? structureStorage.Store : null;
            var memory = structure.GetMemory();
            if (memory == _game.Memory) memory = null;

            var containedResources = structure.ContainedResourceTypes().ToArray();
            
            var name = memory?.TryGetString(StructureName, out var n) ?? false ? n : structure is IWithName s ? s.Name : structure.Id;
            var infoHeight = (/*Titel*/ 1 + /*Storage*/ containedResources.Length + /*Memory*/ CalculateMemoryEntries(memory)) * InfoLineHeight;
            infoHeight += /*Plus Padding*/  ( /*Titel*/ 2 + /*Storage*/ (containedResources.Length != 0 ? 1 : 0) + /*Memory*/ (memory == null ? 0 : 1)) * InfoPadding;

            var startX = structure.LocalPosition.X >= RoomWidth / 2 ? /*left*/ structure.LocalPosition.X - InfoWidth : /*right*/ structure.LocalPosition.X;
            var startY = structure.LocalPosition.Y >= RoomWidth / 2 ? /*top*/ structure.LocalPosition.Y - infoHeight : /*bottom*/ structure.LocalPosition.Y;

            var currentLeftX = startX + InfoPadding / 2;
            var currentRightX = startX + InfoWidth - InfoPadding;
            var currentY = startY + InfoPadding;
            
            _room.Room.Visual.Circle(new FractionalPosition(structure.LocalPosition.X, structure.LocalPosition.Y), StructureMarker);
            
            _room.Room.Visual.Rect(new FractionalPosition(startX, startY), InfoWidth, infoHeight, InfoBox);
            _room.Room.Visual.Text(name, new FractionalPosition(currentLeftX, currentY + InfoLineHeight/2), Title);

            currentY += InfoLineHeight + InfoPadding;
            
            // Storage
            if (containedResources.Length > 0) {
                _room.Room.Visual.HorizontalLine(new FractionalPosition(startX, currentY - InfoLineHeight/2 - InfoPadding/2), InfoWidth, InfoSeparator);
                _room.Room.Visual.StorageTextBlock(structure, currentLeftX, currentY, InfoWidth - 2 * InfoPadding, InfoLineHeight, out var newY, LeftColumn, RightColumn);
                currentY = newY + InfoPadding;
            }
            
            // Memory
            if (memory != null && memory.Keys.Any()) {
                _room.Room.Visual.HorizontalLine(new FractionalPosition(startX, currentY - InfoLineHeight/2 - InfoPadding/2), InfoWidth, InfoSeparator);
                _room.Room.Visual.MemoryTextBlock(memory, currentLeftX, currentY, InfoWidth - 2 * InfoPadding, InfoLineHeight, out var newY, LeftColumn, RightColumn);
                currentY = newY + InfoPadding;
            }
        }
    }

    private static int CalculateMemoryEntries(IMemoryObject? memory) {
        var sum = 0;
        if (memory == null) return sum;
        foreach (var key in memory.Keys) {
            sum += 1;
            if (memory.TryGetObject(key, out var o)) {
                sum += CalculateMemoryEntries(o);
            }
        }
        return sum;
    }
}