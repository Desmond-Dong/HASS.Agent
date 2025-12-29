using Xunit;
using FluentAssertions;
using HASS.Agent.Shared.Enums;

namespace HASS.Agent.Tests.Enums
{
    public class ComponentStatusTests
    {
        [Fact]
        public void ComponentStatus_Values_ShouldBeDefined()
        {
            // Arrange & Act & Assert
            var statusValues = Enum.GetValues(typeof(ComponentStatus));
            statusValues.Length.Should().BeGreaterThan(0, "ComponentStatus should have defined values");
        }

        [Theory]
        [InlineData(ComponentStatus.Ready)]
        [InlineData(ComponentStatus.Loading)]
        [InlineData(ComponentStatus.Error)]
        [InlineData(ComponentStatus.Disabled)]
        public void ComponentStatus_ShouldHaveValidValues(ComponentStatus status)
        {
            // Arrange & Act & Assert
            Enum.IsDefined(typeof(ComponentStatus), status).Should().BeTrue();
        }
    }
}
