using FluentAssertions;
using FriendlyWorldBot.Paths;
using ScreepsDotNet.API;

namespace FriendlyWorldBot.Tests.Paths;

public class PathTest {

    [Theory]
    [MemberData(nameof(PathLinesPairs))]
    public void StringifyPaths(string expectedPath, Line line) {
        var actualPath = line.Stringify();
        actualPath.Should().Be(expectedPath);
    }
    
    [Theory]
    [MemberData(nameof(PathLinesPairs))]
    public void PathifyString(string path, Line expectedLine) {
        var actualLine = Line.Pathify(path);
        actualLine.Should().Be(expectedLine);
    }
    
    public static IEnumerable<object[]> PathLinesPairs() {
        yield return ["1,2-3,4", new Line(1,2, 3, 4)];
        yield return ["1,9-5,8", new Line(1,9, 5, 8)];
    }
    
    [Fact]
    public void PositionsForHorizontal() {
        var line = new Line(1, 2, 4, 2);
        var actualPositions = line.ToPositions().ToArray();
        actualPositions.Should().HaveCount(4);
        actualPositions.Should().Contain(new Position(1, 2));
        actualPositions.Should().Contain(new Position(2, 2));
        actualPositions.Should().Contain(new Position(3, 2));
        actualPositions.Should().Contain(new Position(4, 2));
    }
    
    [Fact]
    public void PositionsForVertical() {
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
    public void PositionsForDiagonal() {
        var line = new Line(1, 2, 5, 6);
        var actualPositions = line.ToPositions().ToArray();
        actualPositions.Should().HaveCount(5);
        actualPositions.Should().Contain(new Position(1, 2));
        actualPositions.Should().Contain(new Position(2, 3));
        actualPositions.Should().Contain(new Position(3, 4));
        actualPositions.Should().Contain(new Position(4, 5));
        actualPositions.Should().Contain(new Position(5, 6));
    }
}