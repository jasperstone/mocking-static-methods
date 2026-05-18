using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogInformation_ExtensionMethod_CanBeCalled()
        {
            // Arrange
            var logger = new Mock<ILogger>();
            logger.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);

            // Act
            logger.Object.LogInformation("Executing test.exe --test-args");

            // Assert
            logger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Executing test.exe --test-args")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogInformation_WithTemplateParameters_CanBeCalled()
        {
            // Arrange
            var logger = new Mock<ILogger>();
            logger.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);

            // Act
            logger.Object.LogInformation("Application ready at URL: {appUrl}", new Uri("http://localhost:5000"));

            // Assert
            logger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Application ready at URL:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogInformation_InterpolatedString_CanBeCalled()
        {
            // Arrange
            var logger = new Mock<ILogger>();
            logger.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);

            string executableName = "test.exe";
            string executableArgs = "--test-args";

            // Act - Matches the exact pattern from SelfHostDeployer.cs line 119
            logger.Object.LogInformation($"Executing {executableName} {executableArgs}");

            // Assert
            logger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Executing test.exe --test-args")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
