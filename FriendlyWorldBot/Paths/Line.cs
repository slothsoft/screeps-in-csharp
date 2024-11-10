using System;
using System.Collections.Generic;
using System.Linq;
using ScreepsDotNet.API;
using static FriendlyWorldBot.Paths.PathExtensions;

namespace FriendlyWorldBot.Paths;

public record Line(int StartX, int StartY, int EndX, int EndY) : IPath {

    internal const string SeparatorTo = "-";
    
    public static Line Pathify(string someString) {
        var startEnd = someString.Split(SeparatorTo);
        var start = startEnd.First().Split(SeparatorXy);
        var end = startEnd.Last().Split(SeparatorXy);
        return new Line(int.Parse(start.First()), int.Parse(start.Last()), int.Parse(end.First()), int.Parse(end.Last()));
    }
    
    public string Stringify() {
        return $"{StartX}{SeparatorXy}{StartY}{SeparatorTo}{EndX}{SeparatorXy}{EndY}";
    }

    public IEnumerable<Position> ToPositions() {
        var xDiff = EndX - StartX;
        var yDiff = EndY - StartY;
        var biggerDiff = Math.Max(xDiff, yDiff);
        var stepX = (double) xDiff / biggerDiff;
        var stepY = (double) yDiff / biggerDiff;
        
        for (var i = 0; i <= biggerDiff; i++) {
            yield return new Position((int) Math.Round(StartX + stepX * i), (int) Math.Round(StartY + stepY * i));
        }
    }

    public bool Contains(int x, int y) {
        return ToPositions().Any(p => p.X == x && p.Y == y);
    }
}