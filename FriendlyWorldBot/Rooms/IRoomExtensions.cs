using System.Collections.Generic;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Rooms;

public static class IRoomExtensions {

    public static IEnumerable<PathStep> FindWalkingPath(this IRoom room, Position from, Position to) {
        return room.FindPath(new RoomPosition(from, room.Coord), new RoomPosition(to, room.Coord), new FindPathOptions(
                ignoreCreeps: true,
                ignoreRoads: true,
                maxRooms: 1
        ));
    }
}