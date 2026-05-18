using System;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class CliService_LogNewVersionInfo_Tests
    {
        [Theory]
        [InlineData(CliService.UpdateChannel.Stable, "-g", "dotnet tool update -g Volo.Abp.Cli")]
        [InlineData(CliService.UpdateChannel.Stable, "--tool-path C:\\tools", "dotnet tool update --tool-path C:\\tools Volo.Abp.Cli")]
        [InlineData(CliService.UpdateChannel.Prerelease, "-g", "dotnet tool update -g Volo.Abp.Cli --version 1.2.3")]
        [InlineData(CliService.UpdateChannel.Nightly, "-g", "dotnet tool uninstall -g Volo.Abp.Cli")]
        [InlineData(CliService.UpdateChannel.Development, "-g", "dotnet tool uninstall -g Volo.Abp.Cli")]
        public void LogNewVersionInfo_LogsExpectedWarningMessages(
            CliService.UpdateChannel updateChannel,
            string toolPath,
            string expectedLastWarning)
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CliService>>();
            var cliService = new CliService(
                commandLineArgumentParser: null,
                commandSelector: null,
                serviceScopeFactory: null,
                nugetService: null,
                cmdHelper: null,
                memoryService: null,
                cliVersionService: null,
                telemetryService: null)
            {
                Logger = loggerMock.Object
            };

            // We need to override IsGlobalTool to control toolPathArg
            // But IsGlobalTool is private, so we simulate by passing toolPath that matches global or not
            // The method IsGlobalTool checks for "%USERPROFILE%\.dotnet\tools\" or "%HOME%/.dotnet/tools/"
            // We will test with toolPath that matches those expanded env vars or not
            // So for "-g" toolPath, we simulate global tool path, for others, non-global

            // Act
            // We call the private method LogNewVersionInfo via reflection because it's private
            var method = typeof(CliService).GetMethod("LogNewVersionInfo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);

            // Use a SemanticVersion instance for latestVersion
            var latestVersion = new NuGet.Versioning.SemanticVersion(1, 2, 3);

            // Call the method
            method.Invoke(cliService, new object[] { updateChannel, latestVersion, toolPath, "Custom message" });

            // Assert
            // We expect Logger.LogWarning to be called with specific messages including the expectedLastWarning string
            // We verify that the expectedLastWarning string was logged at least once
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(expectedLastWarning)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);

            // Also verify that the initial warning about new version is logged
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("A newer")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);

            // Also verify that the custom message is logged
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Custom message")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }
    }
}
