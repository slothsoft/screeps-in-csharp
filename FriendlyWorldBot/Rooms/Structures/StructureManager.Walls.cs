using System;
using System.Collections.Generic;
using System.Linq;
using FriendlyWorldBot.Utils;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;
using static FriendlyWorldBot.Utils.IMemoryConstants;

namespace FriendlyWorldBot.Rooms.Structures;

public partial class StructureManager
{
    private bool BuildWalls()
    {
        return BuildManualWalls() || BuildSpawnWalls() || BuildAutomaticWalls();
    }

    private bool BuildManualWalls()
    {
        // _game.Constants.Controller.GetMaxStructureCount<>()
        // _room.Room.GetManualBuildConfigPath()
        return false;
    }
    
    private bool BuildSpawnWalls()
    {
        return false;
    }


    private bool BuildAutomaticWalls()
    {
        return false;
    }


    public static bool IsValidWallPosition(Position pos) {
        var absX = Math.Abs(pos.X);
        var absY = Math.Abs(pos.Y);
        if (absX < 2 && absY < 2) {
            // to close to the spawn
            return false;
        }

        if (absX == absY) {
            // these are the diagonal lines from the spawn
            return false;
        }

        if (absY < 2) {
            // the wide horizontal road
            return false;
        }

        if (absX == 0) {
            // the vertical road
            return false;
        }

        return true;
    }
}