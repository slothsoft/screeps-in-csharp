using System;
using System.Collections.Generic;
using ScreepsDotNet.API;

namespace FriendlyWorldBot.Paths;

public record EmptyPath : IPath
{
    internal static readonly IPath Instance = new EmptyPath();
    
    public string Stringify() => string.Empty;

    public IEnumerable<Position> ToPositions() => Array.Empty<Position>();
}