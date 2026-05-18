using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Volo.Abp.Cli;
using NuGet.Versioning;
using System.Reflection;

namespace Volo.Abp.Cli.Tests
{
    public class CliServiceTests
    {
        [Fact]
        public void LogNewVersionInfo_LogsCorrectMessages()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CliService>>();
            var cliService = new CliService(
                null, // CommandLineArgumentParser
                null, // CommandSelector
                null, // ServiceScopeFactory
                null, // PackageVersionCheckerService
                null, // CmdHelper
                null, // MemoryService
                null, // CliVersionService
                null  // TelemetryService
            )
            {
                Logger = loggerMock.Object
            };

            // Use reflection to access the protected enum
            var updateChannelField = typeof(CliService).GetField("UpdateChannel", BindingFlags.NonPublic | BindingFlags.Static);
            var updateChannel = (CliService.UpdateChannel)updateChannelField.GetValue(null);
            var latestVersion = new SemanticVersion(2, 0, 0);
            var toolPath = "/path/to/tool";

            // Act
            cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(s => s.Contains("A newer stable version of the ABP CLI is available: 2.0.0.")),
                    It.IsAny<Exception>()
                ),
                Times.Once
            );

            loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(s => s.Contains("dotnet tool update -g Volo.Abp.Cli")),
                    It.IsAny<Exception>()
                ),
                Times.Once
            );

            loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(s => s == string.Empty),
                    It.IsAny<Exception>()
                ),
                Times.Exactly(3) // Three empty lines logged
            );

            loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(s => s.Contains("Update Command:")),
                    It.IsAny<Exception>()
                ),
                Times.Once
            );
        }
    }
}
