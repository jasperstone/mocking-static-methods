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
        public void LogNewVersionInfo_LogsCorrectMessages_ForStableUpdateChannel()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CliService>>();
            var cliService = new CliService(
                null, null, null, null, null, null, null, null, loggerMock.Object);

            var updateChannel = UpdateChannel.Stable;
            var latestVersion = new SemanticVersion(2, 0, 0);
            var toolPath = "/path/to/tool";

            // Act
            cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(s => s.Contains("A newer stable version of the ABP CLI is available: 2.0.0.")),
                    It.IsAny<Exception>()
                ), Times.Once);

            loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(s => s.Contains("dotnet tool update -g Volo.Abp.Cli")),
                    It.IsAny<Exception>()
                ), Times.Once);

            loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(s => s.Contains("Update Command:")),
                    It.IsAny<Exception>()
                ), Times.Once);
        }
    }
}
