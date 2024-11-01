using FluentAssertions;
using FriendlyWorldBot.Paths;
using ScreepsDotNet.API;

namespace FriendlyWorldBot.Tests.Paths;

public class PathTest {

    [Theory]
    [MemberData(nameof(PathStringPairs))]
    public void StringifyPaths(string expectedPath, IPath path) {
        var actualPath = path.Stringify();
        actualPath.Should().Be(expectedPath);
    }
    
    [Theory]
    [MemberData(nameof(PathStringPairs))]
    public void PathifyString(string path, IPath expectedPath) {
        var actualLine = path.Pathify();
        actualLine.Should().Be(expectedPath);
    }
    
    public static IEnumerable<object[]> PathStringPairs() {
        yield return ["1,2-3,4", new Line(1,2, 3, 4)];
        yield return ["1,9-5,8", new Line(1,9, 5, 8)];
        yield return ["1,2~3,4", new Rectangle(1,2, 3, 4)];
        yield return ["1,9~5,8", new Rectangle(1,9, 5, 8)];
    }
    
    [Fact]
    public void LinePositionsForHorizontal() {
        var line = new Line(1, 2, 4, 2);
        var actualPositions = line.ToPositions().ToArray();
        actualPositions.Should().HaveCount(4);
        actualPositions.Should().Contain(new Position(1, 2));
        actualPositions.Should().Contain(new Position(2, 2));
        actualPositions.Should().Contain(new Position(3, 2));
        actualPositions.Should().Contain(new Position(4, 2));
    }
    
    [Fact]
    public void LinePositionsForVertical() {
        var line = new Line(1, 2, 1, 7);
        var actualPositions = line.ToPositions().ToArray();
        actualPositions.Should().HaveCount(6);
        actualPositions.Should().Contain(new Position(1, 2));
        actualPositions.Should().Contain(new Position(1, 3));
        actualPositions.Should().Contain(new Position(1, 4));
        actualPositions.Should().Contain(new Position(1, 5));
        actualPositions.Should().Contain(new Position(1, 6));
        actualPositions.Should().Contain(new Position(1, 7));
    }
    
    [Fact]
    public void LinePositionsForDiagonal() {
        var line = new Line(1, 2, 5, 6);
        var actualPositions = line.ToPositions().ToArray();
        actualPositions.Should().HaveCount(5);
        actualPositions.Should().Contain(new Position(1, 2));
        actualPositions.Should().Contain(new Position(2, 3));
        actualPositions.Should().Contain(new Position(3, 4));
        actualPositions.Should().Contain(new Position(4, 5));
        actualPositions.Should().Contain(new Position(5, 6));
    }
    
    [Fact]
    public void RectanglePositions() {
        var rect = new Rectangle(6, 7, 8, 9);
        var actualPositions = rect.ToPositions().ToArray();
        actualPositions.Should().HaveCount(9);
        actualPositions.Should().Contain(new Position(6, 7));
        actualPositions.Should().Contain(new Position(7, 7));
        actualPositions.Should().Contain(new Position(8, 7));
        
        actualPositions.Should().Contain(new Position(6, 8));
        actualPositions.Should().Contain(new Position(7, 8));
        actualPositions.Should().Contain(new Position(8, 8));
        
        actualPositions.Should().Contain(new Position(6, 9));
        actualPositions.Should().Contain(new Position(7, 9));
        actualPositions.Should().Contain(new Position(8, 9));
    }
}