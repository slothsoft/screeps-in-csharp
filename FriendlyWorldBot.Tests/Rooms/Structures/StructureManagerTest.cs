using FluentAssertions;
using Moq;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;
using Xunit.Abstractions;

namespace FriendlyWorldBot.Rooms.Structures;

public class StructureManagerTest {
    private readonly ITestOutputHelper _testOutputHelper;

    public StructureManagerTest(ITestOutputHelper testOutputHelper) {
        _testOutputHelper = testOutputHelper;
    }

    [Theory]
    // to close to the spawn
    [InlineData(0, 0, false)]
    [InlineData(-1, 0, false)]
    [InlineData(1, 0, false)]
    [InlineData(0, -1, false)]
    [InlineData(0, 1, false)]
    [InlineData(-1, 1, false)]
    [InlineData(1, 1, false)]
    [InlineData(-1, -1, false)]
    [InlineData(1, -1, false)]
    // these are the diagonal lines from the spawn
    [InlineData(2, -2, false)]
    [InlineData(2, 2, false)]
    [InlineData(-2, -2, false)]
    [InlineData(-2, 2, false)]
    [InlineData(3, -3, false)]
    [InlineData(3, 3, false)]
    [InlineData(-3, -3, false)]
    [InlineData(-3, 3, false)]
    // the wide horizontal road
    [InlineData(-3, -1, false)]
    [InlineData(3, 1, false)]
    // vertical road
    [InlineData(0, -3, false)]
    [InlineData(0, 2, false)]
    // so these work
    [InlineData(-1, -2, true)]
    [InlineData(-1, 2, true)]
    [InlineData(1, -2, true)]
    [InlineData(1, 2, true)]
    [InlineData(-3, -2, true)]
    [InlineData(-3, 2, true)]
    [InlineData(3, -2, true)]
    [InlineData(3, 2, true)]
    [InlineData(-4, -2, true)]
    [InlineData(-4, 2, true)]
    [InlineData(4, -2, true)]
    [InlineData(4, 2, true)]
    [InlineData(-1, 3, true)]
    [InlineData(1, 3, true)]
    [InlineData(-1, 4, true)]
    [InlineData(1, 4, true)]
    [InlineData(-1, -3, true)]
    [InlineData(1, -3, true)]
    [InlineData(-1, -4, true)]
    [InlineData(1, -4, true)]
    [InlineData(-2, 4, true)]
    [InlineData(2, 4, true)]
    [InlineData(-2, -4, true)]
    [InlineData(2, -4, true)]
    public void IsValidPosition(int x, int y, bool expected) {
        var actual = StructureManager.IsValidPosition(new Position(x, y));
        actual.Should().Be(expected);
    }

    [Fact]
    public void CreateExtensionPositions() {
        var spawn = new Mock<IStructureSpawn>();
        spawn.Setup(s => s.LocalPosition).Returns(new Position(3, 7));

        var number = 50;
        var actual = StructureManager.CreateExtensionPositions([ spawn.Object ], number).ToList();
        actual.Should().HaveCountGreaterOrEqualTo(number);
        actual.Should().Contain(new Position(2, 5));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 5)]
    [InlineData(2, 10)]
    [InlineData(3, 20)]
    [InlineData(4, 30)]
    [InlineData(5, 40)]
    [InlineData(6, 50)]
    [InlineData(7, 60)]
    public void GetPossibleExtensionCount(int level, int expected) {
        var actual = StructureManager.GetPossibleExtensionCount(level);
        actual.Should().Be(expected);
    }
}