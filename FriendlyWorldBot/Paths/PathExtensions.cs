namespace FriendlyWorldBot.Paths;

public static class PathExtensions {
    internal const string SeparatorXy = ",";
    
    public static IPath Pathify(this string someString) {
        if (string.IsNullOrWhiteSpace(someString))
        {
            return EmptyPath.Instance;
        } 
        if (someString.Contains(PathCollection.SeparatorPaths))
        {
            return PathCollection.Pathify(someString);
        } 
        if (someString.Contains(Line.SeparatorTo))
        {
            return Line.Pathify(someString);
        } 
        if (someString.Contains(Rectangle.SeparatorTo))
        {
            return Rectangle.Pathify(someString);
        } 
        return Point.Pathify(someString);
    }
}