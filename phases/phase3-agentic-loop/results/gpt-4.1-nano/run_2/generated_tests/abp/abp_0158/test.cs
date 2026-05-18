using System;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli;

namespace Volo.Abp.Cli.Tests
{
    public class CliServiceTests
    {
        [Fact]
        public void LogNewVersionInfo_Should_Call_LogWarning_With_Correct_Message()
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
                null  // ITelemetryService
            )
            {
                Logger = loggerMock.Object
            };

            var updateChannel = CliService.UpdateChannel.Stable;
            var version = new NuGet.Versioning.SemanticVersion(1, 2, 3);
            var toolPath = "somePath";

            // Act
            cliService.LogNewVersionInfo(updateChannel, version, toolPath, "Test message");

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(It.Is<string>(msg => msg.Contains("A newer"))),
                Times.AtLeastOnce);
        }
    }
}
