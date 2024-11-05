using FriendlyWorldBot.Utils;
using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Rooms;

public class MemoryManager : IManager {
    private const int CleanEveryTicks = 60;
    private readonly IGame _game;

    public MemoryManager(IGame game) {
        _game = game;
    }

    public void Tick() {
        if ((_game.Time + 15) % CleanEveryTicks != 0) return;
        _game.CleanMemory();
    }
}