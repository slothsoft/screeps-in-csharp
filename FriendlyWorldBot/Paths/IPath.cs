using System.Collections.Generic;
using ScreepsDotNet.API;

namespace FriendlyWorldBot.Paths;

public interface IPath {

    string Stringify();
    IEnumerable<Position> ToPositions();
    bool Contains(int x, int y);
}