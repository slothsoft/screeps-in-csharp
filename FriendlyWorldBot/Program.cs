using System;
using System.Diagnostics.CodeAnalysis;
using FriendlyWorldBot.Utils;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet;

public static partial class Program {
    private static API.World.IGame? _game;
    private static API.Bot.IBot? _bot;

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(Program))]
    public static void Main() {
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    public static void Init() {
        try {
            _game = new Native.World.NativeGame();
            _bot = new FriendlyWorldBot.WorldBot(_game);
        } catch (Exception e) {
            Console.WriteLine(e);
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    public static void Loop() {
        try {
            _game?.Tick();
            _bot?.Loop();
            _game!.Cpu.GetHeapStatistics().CheckHeap();
            MemoryUtil.LogGcActivity();
        } catch (Exception e) {
            Console.WriteLine(e);
        }
    }

    public static IGame? Game => _game;
}