using ScreepsDotNet.API;

namespace FriendlyWorldBot.Gui;

public interface IGuiConstants {
    public const int RoomWidth = 50;
    public const int RoomHeight = 50;
    
    public static readonly Color ColorGolden = new(byte.MaxValue, byte.MaxValue, 0);
    public static readonly Color ColorTransparent = new(0, 0, 0, 0);
}