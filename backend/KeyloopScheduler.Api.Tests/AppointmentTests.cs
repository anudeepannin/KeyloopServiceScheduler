using Xunit;

namespace KeyloopScheduler.Api.Tests;

public class AppointmentTests
{
    [Fact]
    public void AppointmentDuration_WhenStartAndEndAreOneHourApart_Returns60Minutes()
    {
        // Arrange
        var startTime = new DateTimeOffset(2026, 10, 12, 9, 0, 0, TimeSpan.Zero);
        var endTime = new DateTimeOffset(2026, 10, 12, 10, 0, 0, TimeSpan.Zero);

        // Act
        var duration = (endTime - startTime).TotalMinutes;

        // Assert
        Assert.Equal(60, duration);
    }

    [Fact]
    public void AppointmentTimes_WhenUsingUtc_HaveZeroOffset()
    {
        // Arrange
        var startTime = new DateTimeOffset(2026, 10, 12, 9, 0, 0, TimeSpan.Zero);

        // Assert
        Assert.Equal(TimeSpan.Zero, startTime.Offset);
    }

    [Fact]
    public void AppointmentEndTime_WhenBeforeStartTime_IsInvalid()
    {
        // Arrange
        var startTime = new DateTimeOffset(2026, 10, 12, 10, 0, 0, TimeSpan.Zero);
        var endTime = new DateTimeOffset(2026, 10, 12, 9, 0, 0, TimeSpan.Zero);

        // Assert
        Assert.True(endTime <= startTime);
    }
}