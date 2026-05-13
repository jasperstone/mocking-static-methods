using System;
using Microsoft.Extensions.Logging;
using Moq;
using NuGet.Versioning;
using Volo.Abp.Cli;
using Xunit;

public class CliServiceTests
{
    [Fact]
    public void LogNewVersionInfo_StableChannel_LogsCorrectUpdateCommand()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CliService>>();
        var cliService = new CliService(
            null, null, null, null, null, null, null, null)
        {
            Logger = loggerMock.Object
        };

        var updateChannel = CliService.UpdateChannel.Stable;
        var latestVersion = new SemanticVersion(2, 0, 0);
        var toolPath = @"C:\path\to\tool";

        // Act
        cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath);

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(s => s.Contains("dotnet tool update --tool-path C:\\path\\to\\tool Volo.Abp.Cli")),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogNewVersionInfo_PrereleaseChannel_LogsCorrectUpdateCommand()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CliService>>();
        var cliService = new CliService(
            null, null, null, null, null, null, null, null)
        {
            Logger = loggerMock.Object
        };

        var updateChannel = CliService.UpdateChannel.Prerelease;
        var latestVersion = new SemanticVersion(2, 0, 0);
        var toolPath = @"C:\path\to\tool";

        // Act
        cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath);

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(s => s.Contains("dotnet tool update --tool-path C:\\path\\to\\tool Volo.Abp.Cli --version 2.0.0")),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogNewVersionInfo_NightlyChannel_LogsCorrectUpdateCommand()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CliService>>();
        var cliService = new CliService(
            null, null, null, null, null, null, null, null)
        {
            Logger = loggerMock.Object
        };

        var updateChannel = CliService.UpdateChannel.Nightly;
        var latestVersion = new SemanticVersion(2, 0, 0);
        var toolPath = @"C:\path\to\tool";

        // Act
        cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath);

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(s => s.Contains("dotnet tool uninstall --tool-path C:\\path\\to\\tool Volo.Abp.Cli")),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(s => s.Contains("dotnet tool install --tool-path C:\\path\\to\\tool Volo.Abp.Cli --add-source https://www.myget.org/F/abp-nightly/api/v3/index.json --version 2.0.0")),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogNewVersionInfo_DevelopmentChannel_LogsCorrectUpdateCommand()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CliService>>();
        var cliService = new CliService(
            null, null, null, null, null, null, null, null)
        {
            Logger = loggerMock.Object
        };

        var updateChannel = CliService.UpdateChannel.Development;
        var latestVersion = new SemanticVersion(2, 0, 0);
        var toolPath = @"C:\path\to\tool";

        // Act
        cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath);

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(s => s.Contains("dotnet tool uninstall --tool-path C:\\path\\to\\tool Volo.Abp.Cli")),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(s => s.Contains("dotnet tool install --tool-path C:\\path\\to\\tool Volo.Abp.Cli --add-source https://www.myget.org/F/abp-nightly/api/v3/index.json --version 2.0.0")),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
