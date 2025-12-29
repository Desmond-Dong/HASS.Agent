using Xunit;
using FluentAssertions;
using HASS.Agent.Shared.Extensions;

namespace HASS.Agent.Tests.Extensions
{
    public class DateTimeExtensionsTests
    {
        [Fact]
        public void ToUnixTimeSeconds_ValidDate_ShouldReturnCorrectTimestamp()
        {
            // Arrange
            var date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // Act
            var timestamp = date.ToUnixTimeSeconds();

            // Assert
            timestamp.Should().Be(0);
        }

        [Fact]
        public void ToUnixTimeSeconds_DateTimeNow_ShouldReturnPositiveTimestamp()
        {
            // Arrange
            var date = DateTime.UtcNow;

            // Act
            var timestamp = date.ToUnixTimeSeconds();

            // Assert
            timestamp.Should().BeGreaterThan(0);
        }

        [Fact]
        public void FromUnixTimeSeconds_ValidTimestamp_ShouldReturnCorrectDate()
        {
            // Arrange
            const long timestamp = 0;

            // Act
            var date = timestamp.FromUnixTimeSeconds();

            // Assert
            date.Year.Should().Be(1970);
            date.Month.Should().Be(1);
            date.Day.Should().Be(1);
        }

        [Fact]
        public void FromUnixTimeSeconds_ToUnixTimeSeconds_RoundTrip_ShouldPreserveValue()
        {
            // Arrange
            var originalDate = DateTime.UtcNow;

            // Act
            var timestamp = originalDate.ToUnixTimeSeconds();
            var resultDate = timestamp.FromUnixTimeSeconds();

            // Assert
            resultDate.Should().BeCloseTo(originalDate, TimeSpan.FromSeconds(1));
        }
    }
}
