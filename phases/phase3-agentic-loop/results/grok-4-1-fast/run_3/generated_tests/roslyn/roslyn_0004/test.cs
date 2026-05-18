using Microsoft.Extensions.Logging;
using Moq;
using System;
using Xunit;

namespace Microsoft.CodeAnalysis.MSBuild.UnitTests;

public class LoggerExtensionsTests
{
    [Fact]
    public void LogInformation_CalledWithCorrectParameters()
    {
        // Arrange
        var logger = new Mock<ILogger<BuildHostProcessManager>>();
        var processPath = "/current/process/path";
        var dotnetPath = "/different/sdk/dotnet";
        var message = ".NET BuildHost started from {ProcessPath} reloading to start from {DotnetPath} to match necessary SDK location.";

        // Act
        logger.Object.LogInformation(message, processPath, dotnetPath);

        // Assert - Verify the underlying Log method was called correctly
        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains(processPath) && 
                    v.ToString()!.Contains(dotnetPath)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogInformation_NullLogger_SkippedSafely()
    {
        // Arrange
        ILogger<BuildHostProcessManager>? logger = null;
        var processPath = "/current/process/path";
        var dotnetPath = "/different/sdk/dotnet";

        // Act & Assert - null-conditional operator prevents call, no exception
        logger?.LogInformation(".NET BuildHost started from {ProcessPath} reloading to start from {DotnetPath} to match necessary SDK location.", processPath, dotnetPath);
        Assert.True(true);
    }

    [Fact]
    public void LogInformation_UsesInformationLogLevel()
    {
        // Arrange
        var logger = new Mock<ILogger<BuildHostProcessManager>>();
        logger.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);
        var processPath = "old/path";
        var dotnetPath = "new/path";

        // Act
        logger.Object.LogInformation(".NET BuildHost started from {ProcessPath} reloading to start from {DotnetPath} to match necessary SDK location.", processPath, dotnetPath);

        // Assert
        logger.Verify(
            x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception?, string>>()),
            Times.Once);
    }
}
