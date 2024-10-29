using System.Collections.Generic;
using System.Linq;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace FriendlyWorldBot.Rooms.Structures;

public static class StructureExtensions {
    public static TStructure? FindNearest<TStructure>(this IEnumerable<TStructure> structures, Position pos)
        where TStructure : IRoomObject
    {
        return structures
            .Where(static x => x.Exists)
            .MinBy(x => x.LocalPosition.LinearDistanceTo(pos));
    }
}