using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using NuGet.Versioning;
using Volo.Abp.Cli;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class CliService_LogNewVersionInfo_Tests
    {
        private class TestableCliService : CliService
        {
            public TestableCliService(ILogger<CliService> logger) : base(
                commandLineArgumentParser: null,
                commandSelector: null,
                serviceScopeFactory: null,
                nugetService: null,
                cmdHelper: null,
                memoryService: null,
                cliVersionService: null,
                telemetryService: null)
            {
                Logger = logger;
            }

            // Expose protected enum for testing
            public new enum UpdateChannel
            {
                Development,
                Stable,
                Prerelease,
                Nightly
            }

            // Expose IsGlobalTool for testing
            public bool IsGlobalToolPublic(string toolPath) => base.IsGlobalTool(toolPath);

            // Expose LogNewVersionInfo for testing
            public void LogNewVersionInfoPublic(UpdateChannel updateChannel, SemanticVersion latestVersion, string toolPath, string message = null)
                => base.LogNewVersionInfo((CliService.UpdateChannel)(object)updateChannel, latestVersion, toolPath, message);
        }

        [Theory]
        [InlineData(TestableCliService.UpdateChannel.Stable, @"%USERPROFILE%\.dotnet\tools\", "-g", "dotnet tool update -g Volo.Abp.Cli")]
        [InlineData(TestableCliService.UpdateChannel.Stable, @"C:\tools", "--tool-path C:\\tools", "dotnet tool update --tool-path C:\\tools Volo.Abp.Cli")]
        [InlineData(TestableCliService.UpdateChannel.Prerelease, @"%USERPROFILE%\.dotnet\tools\", "-g", "dotnet tool update -g Volo.Abp.Cli --version 1.2.3")]
        [InlineData(TestableCliService.UpdateChannel.Nightly, @"%USERPROFILE%\.dotnet\tools\", "-g", "dotnet tool uninstall -g Volo.Abp.Cli")]
        [InlineData(TestableCliService.UpdateChannel.Development, @"%USERPROFILE%\.dotnet\tools\", "-g", "dotnet tool uninstall -g Volo.Abp.Cli")]
        public void LogNewVersionInfo_LogsExpectedWarnings(TestableCliService.UpdateChannel updateChannel, string toolPathInput, string expectedToolPathArg, string expectedCommand)
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CliService>>();
            var cliService = new TestableCliService(loggerMock.Object);

            // Act
            var latestVersion = new SemanticVersion(1, 2, 3);
            var expandedToolPath = Environment.ExpandEnvironmentVariables(toolPathInput);
            cliService.LogNewVersionInfoPublic(updateChannel, latestVersion, expandedToolPath, "Extra message");

            // Assert
            loggerMock.Verify(l => l.LogWarning($"A newer {updateChannel.ToString().ToLowerInvariant()} version of the ABP CLI is available: {latestVersion}."), Times.Once);
            loggerMock.Verify(l => l.LogWarning("Extra message"), Times.Once);
            loggerMock.Verify(l => l.LogWarning(string.Empty), Times.Exactly(2));
            loggerMock.Verify(l => l.LogWarning("Update Command: "), Times.Once);

            if (updateChannel == TestableCliService.UpdateChannel.Stable)
            {
                loggerMock.Verify(l => l.LogWarning(expectedCommand), Times.Once);
            }
            else if (updateChannel == TestableCliService.UpdateChannel.Prerelease)
            {
                loggerMock.Verify(l => l.LogWarning(expectedCommand), Times.Once);
            }
            else if (updateChannel == TestableCliService.UpdateChannel.Nightly || updateChannel == TestableCliService.UpdateChannel.Development)
            {
                loggerMock.Verify(l => l.LogWarning($"dotnet tool uninstall {expectedToolPathArg} Volo.Abp.Cli"), Times.Once);
                loggerMock.Verify(l => l.LogWarning($"dotnet tool install {expectedToolPathArg} Volo.Abp.Cli --add-source https://www.myget.org/F/abp-nightly/api/v3/index.json --version {latestVersion}"), Times.Once);
            }
        }
    }
}
