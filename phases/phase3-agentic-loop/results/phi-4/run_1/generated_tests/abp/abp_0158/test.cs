using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Volo.Abp.Cli;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Memory;
using Volo.Abp.Cli.Version;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Internal.Telemetry;
using NuGet.Versioning;

namespace Volo.Abp.Cli.Tests
{
    public class CliServiceTests
    {
        private class TestableCliService : CliService
        {
            public TestableCliService(
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
        }

        [Fact]
        public void LogNewVersionInfo_LogsCorrectMessages()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CliService>>();
            var cliService = new TestableCliService(
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

            var updateChannel = CliService.UpdateChannel.Stable;
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
