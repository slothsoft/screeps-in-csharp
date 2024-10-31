using System;
using System.Collections.Generic;
using System.Linq;
using FriendlyWorldBot.Paths;
using FriendlyWorldBot.Utils;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;
using static FriendlyWorldBot.Utils.IMemoryConstants;

namespace FriendlyWorldBot.Rooms;

public static class RoomExtensions {
    public static IEnumerable<PathStep> FindWalkingPath(this IRoom room, Position from, Position to) {
        return room.FindPath(new RoomPosition(from, room.Coord), new RoomPosition(to, room.Coord), new FindPathOptions(
            ignoreCreeps: true,
            ignoreRoads: true,
            maxRooms: 1
        ));
    }

    public static RoomCreateConstructionSiteResult CreateConstructionSite<TStructure>(this IRoom room, Position position,
        IDictionary<string, string> futureMemory)
        where TStructure : class, IStructure {
        var constructionResult = room.CreateConstructionSite<TStructure>(position);
        if (constructionResult == RoomCreateConstructionSiteResult.Ok) {
            if (futureMemory.Count != 0) {
                // store future memory for later
                var memory = room.FetchFutureMemory(position);
                foreach (var keyValue in futureMemory) {
                    memory.SetValue(keyValue.Key, keyValue.Value);
                }
            }
        }

        return constructionResult;
    }

    public static IMemoryObject FetchFutureMemory(this IRoom room, Position position)
        => room.Memory.GetOrCreateObject(RoomFutureMemory).GetOrCreateObject(new Point(position.X, position.Y).Stringify());
    
    public static IEnumerable<Position> FetchFutureMemoryPositions(this IRoom room)
        => room.Memory.GetOrCreateObject(RoomFutureMemory).Keys.Select(p => {
            try {
                var point = Point.Pathify(p);
                return new Position(point.X, point.Y);
            } catch (Exception e) {
                Logger.Instance.Error($"Could not parse future memory ({p})");
                Console.WriteLine(e);
                return (0, 0);
            }
        }).Distinct();
}