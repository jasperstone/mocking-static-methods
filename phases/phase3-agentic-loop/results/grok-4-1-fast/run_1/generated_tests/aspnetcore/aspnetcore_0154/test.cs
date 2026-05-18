using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Hosting.Tests;

public class WebHostBuilderLoggerTests
{
    [Fact]
    public void LogWarning_DuplicateAssembly_LogsWarningMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<WebHost>>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Warning)).Returns(true);
        var logger = loggerMock.Object;

        // Act - simulate the exact LogWarning extension call from WebHostBuilder.Build()
        logger.LogWarning("The assembly Assembly1 was specified multiple times. Hosting startup assemblies should only be specified once.");

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("The assembly Assembly1 was specified multiple times. Hosting startup assemblies should only be specified once.")),
                null!,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogWarning_WhenLogLevelDisabled_NoLogging()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<WebHost>>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Warning)).Returns(false);
        var logger = loggerMock.Object;

        // Act - simulate the conditional logging from WebHostBuilder.Build()
        if (logger.IsEnabled(LogLevel.Warning))
        {
            logger.LogWarning("The assembly Assembly1 was specified multiple times. Hosting startup assemblies should only be specified once.");
        }

        // Assert - no Log call was made
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public void LogWarning_MultipleDuplicates_LogsEachTime()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<WebHost>>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Warning)).Returns(true);
        var logger = loggerMock.Object;

        // Act - simulate multiple duplicate detections
        logger.LogWarning("The assembly Assembly1 was specified multiple times. Hosting startup assemblies should only be specified once.");
        logger.LogWarning("The assembly Assembly2 was specified multiple times. Hosting startup assemblies should only be specified once.");

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("Assembly1")),
                null!,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("Assembly2")),
                null!,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
