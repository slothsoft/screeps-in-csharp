namespace FriendlyWorldBot.Paths;

public static class PathExtensions {

    public static IPath Pathify(this string someString) {
        return Line.Pathify(someString);
    }
}