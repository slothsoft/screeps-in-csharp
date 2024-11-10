using System;
using System.Collections.Generic;
using System.Linq;
using ScreepsDotNet.API;
using static FriendlyWorldBot.Paths.PathExtensions;

namespace FriendlyWorldBot.Paths;

public record Rectangle(int StartX, int StartY, int EndX, int EndY) : IPath {

    internal const string SeparatorTo = "~";
    
    public static Rectangle Pathify(string someString) {
        var startEnd = someString.Split(SeparatorTo);
        var start = startEnd.First().Split(SeparatorXy);
        var end = startEnd.Last().Split(SeparatorXy);
        return new Rectangle(int.Parse(start.First()), int.Parse(start.Last()), int.Parse(end.First()), int.Parse(end.Last()));
    }
    
    public string Stringify() {
        return $"{StartX}{SeparatorXy}{StartY}{SeparatorTo}{EndX}{SeparatorXy}{EndY}";
    }

    public IEnumerable<Position> ToPositions() {
        for (var x = Math.Min(StartX, EndX); x <= Math.Max(StartX, EndX); x++) {
            for (var y = Math.Min(StartY, EndY); y <= Math.Max(StartY, EndY); y++) {
                yield return new Position(x, y);
            }
        }
    }

    public bool Contains(int x, int y) {
        return x >= StartX && x <= EndX && y >= StartY && y <= EndY;
    }
}