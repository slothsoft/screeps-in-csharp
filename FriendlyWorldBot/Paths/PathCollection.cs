using System.Collections.Generic;
using System.Linq;
using ScreepsDotNet.API;

namespace FriendlyWorldBot.Paths;

public record PathCollection(params IPath[] Paths) : IPath
{
    internal const string SeparatorPaths = ";";
    
    public PathCollection(IEnumerable<IPath> paths) : this(paths.ToArray())
    {
    }

    public static PathCollection Pathify(string someString)
    {
        return new PathCollection(someString.Split(SeparatorPaths).Select(PathExtensions.Pathify));
    }

    public string Stringify()
    {
        return string.Join(SeparatorPaths, Paths.Select(p => Stringify()));
    }

    public IEnumerable<Position> ToPositions()
    {
        return Paths.SelectMany(p => p.ToPositions());
    }

    public bool Contains(int x, int y) {
        return Paths.Any(p => p.Contains(x, y));
    }
}