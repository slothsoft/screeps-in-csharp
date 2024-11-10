using System.Collections.Generic;
using System.Linq;
using ScreepsDotNet.API;
using static FriendlyWorldBot.Paths.PathExtensions;

namespace FriendlyWorldBot.Paths;

public record Point(int X, int Y) : IPath {

    public static Point Pathify(string someString) {
        var coords = someString.Split(SeparatorXy);
        return new Point(int.Parse(coords.First()), int.Parse(coords.Last()));
    }
    
    public string Stringify() {
        return $"{X}{SeparatorXy}{Y}";
    }

    public IEnumerable<Position> ToPositions() {
        yield return new Position(X, Y);
    }

    public bool Contains(int x, int y) => X == x && Y == y;
}