using System;
using Microsoft.Extensions.Logging;
using Moq;
using NuGet.Versioning;
using Volo.Abp.Cli;
using Xunit;

public class CliServiceTests
{
    private class TestCliService : CliService
    {
        public TestCliService() : base(null, null, null, null, null, null, null, null)
        {
        }

        public new void LogNewVersionInfo(UpdateChannel updateChannel, SemanticVersion latestVersion, string toolPath, string message = null)
        {
            base.LogNewVersionInfo(updateChannel, latestVersion, toolPath, message);
        }

        public new bool IsGlobalTool(string toolPath)
        {
            return base.IsGlobalTool(toolPath);
        }
    }

    [Fact]
    public void LogNewVersionInfo_StableChannel_LogsCorrectMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CliService>>();
        var cliService = new TestCliService
        {
            Logger = loggerMock.Object
        };

        var updateChannel = CliService.UpdateChannel.Stable;
        var latestVersion = new SemanticVersion(1, 0, 0);
        var toolPath = @"C:\path\to\tool";

        var cliServiceMock = new Mock<TestCliService>();
        cliServiceMock.Setup(x => x.IsGlobalTool(toolPath)).Returns(false);

        // Act
        cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath);

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(m => m.Contains("A newer stable version of the ABP CLI is available: 1.0.0."))),
            Times.Once);

        loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(m => m.Contains("dotnet tool update --tool-path C:\\path\\to\\tool Volo.Abp.Cli"))),
            Times.Once);
    }

    [Fact]
    public void LogNewVersionInfo_PrereleaseChannel_LogsCorrectMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CliService>>();
        var cliService = new TestCliService
        {
            Logger = loggerMock.Object
        };

        var updateChannel = CliService.UpdateChannel.Prerelease;
        var latestVersion = new SemanticVersion(1, 0, 0);
        var toolPath = @"C:\path\to\tool";

        var cliServiceMock = new Mock<TestCliService>();
        cliServiceMock.Setup(x => x.IsGlobalTool(toolPath)).Returns(false);

        // Act
        cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath);

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(m => m.Contains("A newer prerelease version of the ABP CLI is available: 1.0.0."))),
            Times.Once);

        loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(m => m.Contains("dotnet tool update --tool-path C:\\path\\to\\tool Volo.Abp.Cli --version 1.0.0"))),
            Times.Once);
    }

    [Fact]
    public void LogNewVersionInfo_NightlyChannel_LogsCorrectMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CliService>>();
        var cliService = new TestCliService
        {
            Logger = loggerMock.Object
        };

        var updateChannel = CliService.UpdateChannel.Nightly;
        var latestVersion = new SemanticVersion(1, 0, 0);
        var toolPath = @"C:\path\to\tool";

        var cliServiceMock = new Mock<TestCliService>();
        cliServiceMock.Setup(x => x.IsGlobalTool(toolPath)).Returns(false);

        // Act
        cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath);

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(m => m.Contains("A newer nightly version of the ABP CLI is available: 1.0.0."))),
            Times.Once);

        loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(m => m.Contains("dotnet tool uninstall --tool-path C:\\path\\to\\tool Volo.Abp.Cli"))),
            Times.Once);

        loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(m => m.Contains("dotnet tool install --tool-path C:\\path\\to\\tool Volo.Abp.Cli --add-source https://www.myget.org/F/abp-nightly/api/v3/index.json --version 1.0.0"))),
            Times.Once);
    }

    [Fact]
    public void LogNewVersionInfo_DevelopmentChannel_LogsCorrectMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CliService>>();
        var cliService = new TestCliService
        {
            Logger = loggerMock.Object
        };

        var updateChannel = CliService.UpdateChannel.Development;
        var latestVersion = new SemanticVersion(1, 0, 0);
        var toolPath = @"C:\path\to\tool";

        var cliServiceMock = new Mock<TestCliService>();
        cliServiceMock.Setup(x => x.IsGlobalTool(toolPath)).Returns(false);

        // Act
        cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath);

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(m => m.Contains("A newer development version of the ABP CLI is available: 1.0.0."))),
            Times.Once);

        loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(m => m.Contains("dotnet tool uninstall --tool-path C:\\path\\to\\tool Volo.Abp.Cli"))),
            Times.Once);

        loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(m => m.Contains("dotnet tool install --tool-path C:\\path\\to\\tool Volo.Abp.Cli --add-source https://www.myget.org/F/abp-nightly/api/v3/index.json --version 1.0.0"))),
            Times.Once);
    }
}
