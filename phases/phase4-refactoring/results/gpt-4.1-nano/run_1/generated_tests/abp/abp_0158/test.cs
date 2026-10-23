using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli;
using System.Threading.Tasks;
using Volo.Abp.Cli.Version;

namespace Volo.Abp.Cli.Tests
{
    public class CliServiceTests
    {
        [Fact]
        public async Task LogNewVersionInfo_Should_Log_Warning_When_Version_Is_Newer()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<CliService>>();
            var mockTelemetry = new Mock<ITelemetryService>();
            var mockMemory = new Mock<MemoryService>();
            var mockCommandLineParser = new Mock<ICommandLineArgumentParser>();
            var mockCommandSelector = new Mock<ICommandSelector>();
            var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            var mockPackageVersionChecker = new Mock<PackageVersionCheckerService>();
            var mockCmdHelper = new Mock<ICmdHelper>();
            var mockCliVersionService = new Mock<CliVersionService>();

            var cliService = new CliService(
                mockCommandLineParser.Object,
                mockCommandSelector.Object,
                mockServiceScopeFactory.Object,
                mockPackageVersionChecker.Object,
                mockCmdHelper.Object,
                mockMemory.Object,
                mockCliVersionService.Object,
                mockTelemetry.Object
            )
            {
                Logger = mockLogger.Object
            };

            var updateChannel = CliService.UpdateChannel.Stable;
            var latestVersion = new SemanticVersion(1, 2, 3);
            var currentVersion = new SemanticVersion(1, 0, 0);
            var message = "New version available";

            // Act
            cliService.LogNewVersionInfo(updateChannel, latestVersion, "path", message);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"A newer {updateChannel.ToString().ToLowerInvariant()} version of the ABP CLI is available: {latestVersion}.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
