using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Volo.Abp.Cli;
using NuGet.Versioning;

namespace Volo.Abp.Cli.Tests
{
    public class CliServiceTests
    {
        [Fact]
        public void LogNewVersionInfo_ShouldLogCorrectMessages_ForStableChannel()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CliService>>();
            var cliService = new CliService(
                null, null, null, null, null, null, null, null, loggerMock.Object);

            var latestVersion = new SemanticVersion(2, 0, 0);
            var toolPath = "/path/to/tool";

            // Act
            cliService.LogNewVersionInfo(UpdateChannel.Stable, latestVersion, toolPath);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(It.Is<string>(s => s.Contains("A newer stable version of the ABP CLI is available: 2.0.0."))),
                Times.Once);

            loggerMock.Verify(
                x => x.LogWarning(It.Is<string>(s => s.Contains("dotnet tool update --tool-path /path/to/tool Volo.Abp.Cli"))),
                Times.Once);

            loggerMock.Verify(
                x => x.LogWarning(It.Is<string>(s => s.Contains("Update Command: "))),
                Times.Once);

            loggerMock.Verify(
                x => x.LogWarning(string.Empty),
                Times.Exactly(3));
        }

        [Fact]
        public void LogNewVersionInfo_ShouldLogCorrectMessages_ForPrereleaseChannel()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CliService>>();
            var cliService = new CliService(
                null, null, null, null, null, null, null, null, loggerMock.Object);

            var latestVersion = new SemanticVersion(2, 0, 0, "beta");
            var toolPath = "/path/to/tool";

            // Act
            cliService.LogNewVersionInfo(UpdateChannel.Prerelease, latestVersion, toolPath);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(It.Is<string>(s => s.Contains("A newer prerelease version of the ABP CLI is available: 2.0.0-beta."))),
                Times.Once);

            loggerMock.Verify(
                x => x.LogWarning(It.Is<string>(s => s.Contains("dotnet tool update --tool-path /path/to/tool Volo.Abp.Cli --version 2.0.0-beta"))),
                Times.Once);

            loggerMock.Verify(
                x => x.LogWarning(It.Is<string>(s => s.Contains("Update Command: "))),
                Times.Once);

            loggerMock.Verify(
                x => x.LogWarning(string.Empty),
                Times.Exactly(3));
        }

        [Fact]
        public void LogNewVersionInfo_ShouldLogCorrectMessages_ForNightlyChannel()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CliService>>();
            var cliService = new CliService(
                null, null, null, null, null, null, null, null, loggerMock.Object);

            var latestVersion = new SemanticVersion(2, 0, 0);
            var toolPath = "/path/to/tool";

            // Act
            cliService.LogNewVersionInfo(UpdateChannel.Nightly, latestVersion, toolPath);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(It.Is<string>(s => s.Contains("A newer nightly version of the ABP CLI is available: 2.0.0."))),
                Times.Once);

            loggerMock.Verify(
                x => x.LogWarning(It.Is<string>(s => s.Contains("dotnet tool uninstall --tool-path /path/to/tool Volo.Abp.Cli"))),
                Times.Once);

            loggerMock.Verify(
                x => x.LogWarning(It.Is<string>(s => s.Contains("dotnet tool install --tool-path /path/to/tool Volo.Abp.Cli --add-source https://www.myget.org/F/abp-nightly/api/v3/index.json --version 2.0.0"))),
                Times.Once);

            loggerMock.Verify(
                x => x.LogWarning(It.Is<string>(s => s.Contains("Update Command: "))),
                Times.Once);

            loggerMock.Verify(
                x => x.LogWarning(string.Empty),
                Times.Exactly(3));
        }
    }
}
