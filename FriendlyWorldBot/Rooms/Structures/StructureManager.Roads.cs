using System;
using System.Linq;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Rooms.Structures;

public partial class StructureManager
{
    private bool BuildRoads()
    {
        return BuildManualRoads() || BuildSpawnRoads() || BuildAutomaticRoads();
    }

    private bool BuildManualRoads()
    {
        // _game.Constants.Controller.GetMaxStructureCount<>()
        // _room.Room.GetManualBuildConfigPath()
        return false;
    }
    
    private bool BuildSpawnRoads()
    {
        // TODO: this will not work for multiple spawns per room
        var extensions = _room.Extensions;

        var minX = extensions.Select(p => p.LocalPosition.X).Min();
        var minY = extensions.Select(p => p.LocalPosition.Y).Min();
        var maxX = extensions.Select(p => p.LocalPosition.X).Max();
        var maxY = extensions.Select(p => p.LocalPosition.Y).Max();

        var roadCount = 0;
        for (var x = minX; x <= maxX; x++)
        {
            for (var y = minY; y <= maxY; y++)
            {
                if (IsValidRoadPosition(x, y))
                {
                    if (_room.Room.CreateConstructionSite<IStructureExtension>(new Position(x, y)) == RoomCreateConstructionSiteResult.Ok)
                    {
                        roadCount++;
                        if (roadCount > 7) // TODO: magic number
                        {
                            return true;
                        }
                    }      
                }
            }
        }

        return roadCount > 0;
    }
    

    private static bool IsValidRoadPosition(int x, int y) {
        var absX = Math.Abs(x);
        var absY = Math.Abs(y);
        if (absY == 0) {
            // the horizontal not-road is speckled
            return absX % 2 == 1;
        }

        return !IsValidExtensionPosition(x, y);
    }


    private bool BuildAutomaticRoads()
    {
        return false;
    }

}