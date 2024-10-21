using System;
using System.Collections.Generic;
using System.Linq;
using ScreepsDotNet.API;

namespace FriendlyWorldBot.Paths;

public record Line(int startX, int startY, int endX, int endY) : IPath {

    private const string SeparatorTo = "-";
    private const string SeparatorXy = ",";
    
    public static Line Pathify(string someString) {
        var startEnd = someString.Split(SeparatorTo);
        var start = startEnd.First().Split(SeparatorXy);
        var end = startEnd.Last().Split(SeparatorXy);
        return new Line(int.Parse(start.First()), int.Parse(start.Last()), int.Parse(end.First()), int.Parse(end.Last()));
    }
    
    public string Stringify() {
        return $"{startX}{SeparatorXy}{startY}{SeparatorTo}{endX}{SeparatorXy}{endY}";
    }

    public IEnumerable<Position> ToPositions() {
        var xDiff = endX - startX;
        var yDiff = endY - startY;
        var biggerDiff = Math.Max(xDiff, yDiff);
        var stepX = (double) xDiff / biggerDiff;
        var stepY = (double) yDiff / biggerDiff;
        
        for (var i = 0; i <= biggerDiff; i++) {
            yield return new Position((int) Math.Round(startX + stepX * i), (int) Math.Round(startY + stepY * i));
        }
    }
}