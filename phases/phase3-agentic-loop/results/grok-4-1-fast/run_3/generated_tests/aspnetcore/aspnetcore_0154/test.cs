using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using Xunit;

namespace Microsoft.AspNetCore.Hosting.Tests
{
    public class WebHostLoggerWarningTests
    {
        [Fact]
        public void LogWarning_DuplicateAssembly_WhenWarningEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<WebHost>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Warning)).Returns(true);
            var logger = loggerMock.Object;
            var assemblyName = "TestAssembly";
            var expectedMessage = $"The assembly {assemblyName} was specified multiple times. Hosting startup assemblies should only be specified once.";

            // Act
            logger.LogWarning(expectedMessage);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarning_DuplicateAssembly_NoLog_WhenWarningDisabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<WebHost>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Warning)).Returns(false);
            var logger = loggerMock.Object;

            // Act
            logger.LogWarning("Test warning message");

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }
}
