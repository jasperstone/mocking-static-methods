using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli;

namespace AbpCli.Tests
{
    public class CliServiceTests
    {
        [Fact]
        public async Task CheckCliVersionAsync_Should_Log_Warning_When_LatestVersion_Is_Greater()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CliService>>();
            var telemetryMock = new Mock<ITelemetryService>();
            var memoryMock = new Mock<MemoryService>();
            var packageServiceMock = new Mock<PackageVersionCheckerService>();
            var commandLineParserMock = new Mock<ICommandLineArgumentParser>();
            var commandSelectorMock = new Mock<ICommandSelector>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var cliVersionServiceMock = new Mock<CliVersionService>();

            var latestVersion = new SemanticVersion(1, 2, 3);
            var currentVersion = new SemanticVersion(1, 0, 0);
            var latestVersionInfo = new LatestVersionInfo { Version = latestVersion, Message = "Update available" };

            packageServiceMock.Setup(p => p.GetLatestStableVersionFromGithubAsync()).ReturnsAsync(latestVersionInfo);
            // Setup other dependencies as needed...

            var cliService = new CliService(
                commandLineParserMock.Object,
                commandSelectorMock.Object,
                serviceScopeFactoryMock.Object,
                packageServiceMock.Object,
                null,
                memoryMock.Object,
                cliVersionServiceMock.Object,
                telemetryMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await cliService.CheckCliVersionAsync(currentVersion);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(It.Is<string>(s => s.Contains("A newer"))),
                Times.AtLeastOnce);
        }
    }
}
