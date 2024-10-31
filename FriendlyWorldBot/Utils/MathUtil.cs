using System;
using System.Collections.Generic;
using ScreepsDotNet.API;

namespace FriendlyWorldBot.Utils;

public static class MathUtil {
    private static readonly Dictionary<int, Position> UlamSpiral = new();
    
    public static IEnumerable<Position> ToUlamSpiral(this int expectedCount, Func<Position, bool>? isValidPosition = null!) {
        var usedIsValidPosition = isValidPosition ?? (_ => true);
        var remainingCount = expectedCount;
        var number = 1;
        while (remainingCount > 0) {
            var position = CalculateUlamSpiral(number++);
            if (usedIsValidPosition(position)) {
                yield return position;
                remainingCount--;
            }
        }
    }
    
    public static Position CalculateUlamSpiral(this int number) {
        if (UlamSpiral.TryGetValue(number, out var cachedPosition)) {
            return cachedPosition;
        }

        Position result;
        if (number == 1) {
            result = new Position(0, 0);
        } else {
            var numberMinusOnePosition = CalculateUlamSpiral(number - 1);
            result = new Position(
                numberMinusOnePosition.X + (int) +Math.Sin(Math.Floor(Math.Sqrt(4 * number - 7)) * Math.PI / 2),
                numberMinusOnePosition.Y + (int) -Math.Cos(Math.Floor(Math.Sqrt(4 * number - 7)) * Math.PI / 2)
            );
        }
        UlamSpiral.Add(number, result);
        return result;
    }

}