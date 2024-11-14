using System.Linq;
using FriendlyWorldBot.Utils;
using ScreepsDotNet.API.World;
using static FriendlyWorldBot.Utils.IMemoryConstants;

namespace FriendlyWorldBot.Rooms.Structures;

/// <summary>
/// Automatic Builder: https://wiki.screepspl.us/index.php/Automatic_base_building
/// </summary>
public partial class StructureBuilder : IManager
{
    private const int BuildEveryTicks = 60;
    private const int MaxConstructionSites = 7;
    
    private readonly IGame _game;
    private readonly RoomCache _room;

    public StructureBuilder(IGame game, RoomCache room) {
        _game = game;
        _room = room;

        var mainSpawn = room.MainSpawn;
        if (mainSpawn != null) {
            var mainSource = room.Sources.FindNearest(mainSpawn.LocalPosition);
            foreach (var source in room.Sources) {
                source.Room!.Memory.SetValue(RoomMainSource, source.Id);
            }
            Logger.Instance.Info($"set {mainSpawn} as main spawn and {mainSource} as main source");
        }
    }
    
    public void Tick()
    {
        if (_room.Room.Controller == null) return; // not my room
        if (_game.Time % BuildEveryTicks != 0) return;
        if (_room.Room.Find<IConstructionSite>().Count() >= MaxConstructionSites) return;
        
        var somethingWasBuild = BuildExtensions() || BuildRoads() || BuildWalls();
        if (!somethingWasBuild)
        {
            Logger.Instance.Info("Nothing needs to be build.");
        }
    }
}