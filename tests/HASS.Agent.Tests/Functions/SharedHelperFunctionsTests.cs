using Xunit;
using FluentAssertions;
using HASS.Agent.Shared.Functions;

namespace HASS.Agent.Tests.Functions
{
    public class SharedHelperFunctionsTests
    {
        [Theory]
        [InlineData("192.168.1.1", true)]
        [InlineData("10.0.0.1", true)]
        [InlineData("172.16.0.1", true)]
        [InlineData("8.8.8.8", false)]
        [InlineData("1.1.1.1", false)]
        [InlineData("invalid", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void IsPrivateIP_ValidInput_ShouldReturnExpectedResult(string ip, bool expected)
        {
            // Act
            var result = SharedHelperFunctions.IsPrivateIP(ip);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("C:\\Test\\File.txt", "File.txt")]
        [InlineData("/path/to/file.json", "file.json")]
        [InlineData("file", "file")]
        [InlineData("", "")]
        [InlineData(null, "")]
        public void GetFileNameFromPath_ValidInput_ShouldReturnFileName(string path, string expectedFileName)
        {
            // Act
            var result = SharedHelperFunctions.GetFileNameFromPath(path);

            // Assert
            result.Should().Be(expectedFileName);
        }
    }
}
