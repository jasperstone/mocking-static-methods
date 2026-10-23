using System;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli;
using Xunit;

public class CliServiceTests
{
    [Fact]
    public void LogNewVersionInfo_StableChannel_LogsCorrectCommands()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CliService>>();
        var cliService = new CliServiceMock(null, null, null, null, null, null, null, null)
        {
            Logger = loggerMock.Object
        };

        var updateChannel = CliServiceMock.UpdateChannel.Stable;
        var latestVersion = new NuGet.Versioning.SemanticVersion(1, 0, 0);
        var toolPath = @"C:\path\to\tool";

        // Act
        cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath);

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("A newer stable version of the ABP CLI is available: 1.0.0.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        loggerMock.Verify(
            x => x.LogWarning(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Update Command: ")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        loggerMock.Verify(
            x => x.LogWarning(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("dotnet tool update --tool-path C:\\path\\to\\tool Volo.Abp.Cli")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogNewVersionInfo_PrereleaseChannel_LogsCorrectCommands()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CliService>>();
        var cliService = new CliServiceMock(null, null, null, null, null, null, null, null)
        {
            Logger = loggerMock.Object
        };

        var updateChannel = CliServiceMock.UpdateChannel.Prerelease;
        var latestVersion = new NuGet.Versioning.SemanticVersion(1, 0, 0);
        var toolPath = @"C:\path\to\tool";

        // Act
        cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath);

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("A newer prerelease version of the ABP CLI is available: 1.0.0.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        loggerMock.Verify(
            x => x.LogWarning(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Update Command: ")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        loggerMock.Verify(
            x => x.LogWarning(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("dotnet tool update --tool-path C:\\path\\to\\tool Volo.Abp.Cli --version 1.0.0")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogNewVersionInfo_NightlyChannel_LogsCorrectCommands()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CliService>>();
        var cliService = new CliServiceMock(null, null, null, null, null, null, null, null)
        {
            Logger = loggerMock.Object
        };

        var updateChannel = CliServiceMock.UpdateChannel.Nightly;
        var latestVersion = new NuGet.Versioning.SemanticVersion(1, 0, 0);
        var toolPath = @"C:\path\to\tool";

        // Act
        cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath);

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("A newer nightly version of the ABP CLI is available: 1.0.0.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        loggerMock.Verify(
            x => x.LogWarning(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Update Command: ")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        loggerMock.Verify(
            x => x.LogWarning(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("dotnet tool uninstall --tool-path C:\\path\\to\\tool Volo.Abp.Cli")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        loggerMock.Verify(
            x => x.LogWarning(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("dotnet tool install --tool-path C:\\path\\to\\tool Volo.Abp.Cli --add-source https://www.myget.org/F/abp-nightly/api/v3/index.json --version 1.0.0")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogNewVersionInfo_DevelopmentChannel_LogsCorrectCommands()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CliService>>();
        var cliService = new CliServiceMock(null, null, null, null, null, null, null, null)
        {
            Logger = loggerMock.Object
        };

        var updateChannel = CliServiceMock.UpdateChannel.Development;
        var latestVersion = new NuGet.Versioning.SemanticVersion(1, 0, 0);
        var toolPath = @"C:\path\to\tool";

        // Act
        cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath);

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("A newer development version of the ABP CLI is available: 1.0.0.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        loggerMock.Verify(
            x => x.LogWarning(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Update Command: ")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        loggerMock.Verify(
            x => x.LogWarning(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("dotnet tool uninstall --tool-path C:\\path\\to\\tool Volo.Abp.Cli")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        loggerMock.Verify(
            x => x.LogWarning(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("dotnet tool install --tool-path C:\\path\\to\\tool Volo.Abp.Cli --add-source https://www.myget.org/F/abp-nightly/api/v3/index.json --version 1.0.0")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}

public class CliServiceMock : CliService
{
    public CliServiceMock(
        ICommandLineArgumentParser commandLineArgumentParser,
        ICommandSelector commandSelector,
        IServiceScopeFactory serviceScopeFactory,
        PackageVersionCheckerService nugetService,
        ICmdHelper cmdHelper,
        MemoryService memoryService,
        CliVersionService cliVersionService,
        ITelemetryService telemetryService)
        : base(commandLineArgumentParser, commandSelector, serviceScopeFactory, nugetService, cmdHelper, memoryService, cliVersionService, telemetryService)
    {
    }

    public new void LogNewVersionInfo(UpdateChannel updateChannel, SemanticVersion latestVersion, string toolPath, string message = null)
    {
        base.LogNewVersionInfo(updateChannel, latestVersion, toolPath, message);
    }
}
