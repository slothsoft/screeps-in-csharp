using FluentAssertions;
using FriendlyWorldBot.Utils;
using ScreepsDotNet.API;

namespace FriendlyWorldBot.Tests.Utils;

public class MathUtilTest {
    
    [Theory]
    [InlineData(1, 0, 0)]
    [InlineData(2, 1, 0)]
    [InlineData(3, 1, 1)]
    [InlineData(4, 0, 1)]
    [InlineData(5, -1, 1)]
    [InlineData(6, -1, 0)]
    [InlineData(7, -1, -1)]
    [InlineData(8, 0, -1)]
    [InlineData(9, 1, -1)]
    [InlineData(10, 2, -1)]
    [InlineData(11, 2, 0)]
    [InlineData(12, 2, 1)]
    [InlineData(13, 2, 2)]
    [InlineData(14, 1, 2)]
    public void ToUlamSpiral(int number, int expectedLastPositionX, int expectedLastPositionY) {
        var actual = number.ToUlamSpiral();
        var actualLastPosition = actual.Last();
        actualLastPosition.Should().Be(new Position(expectedLastPositionX, expectedLastPositionY));
    }
    
    [Theory]
    [InlineData(1, 0, 0)]
    [InlineData(2, 1, 0)]
    [InlineData(3, 1, 1)]
    [InlineData(4, 0, 1)]
    [InlineData(5, -1, 1)]
    [InlineData(6, -1, 0)]
    [InlineData(7, -1, -1)]
    [InlineData(8, 0, -1)]
    [InlineData(9, 1, -1)]
    [InlineData(10, 2, -1)]
    [InlineData(11, 2, 0)]
    [InlineData(12, 2, 1)]
    [InlineData(13, 2, 2)]
    [InlineData(14, 1, 2)]
    public void CalculateUlamSpiral(int number, int expectedPositionX, int expectedPositionY) {
        var actual = MathUtil.CalculateUlamSpiral(number);
        actual.Should().Be(new Position(expectedPositionX, expectedPositionY));
    }
}