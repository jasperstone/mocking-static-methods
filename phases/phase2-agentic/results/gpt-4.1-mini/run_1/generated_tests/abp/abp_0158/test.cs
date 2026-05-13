using System;
using Microsoft.Extensions.Logging;
using Moq;
using NuGet.Versioning;
using Volo.Abp.Cli;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class CliService_LogNewVersionInfo_Tests
    {
        private readonly Mock<ILogger<CliService>> _loggerMock;
        private readonly CliService _cliService;

        public CliService_LogNewVersionInfo_Tests()
        {
            // We need to mock dependencies for CliService constructor, but only Logger is used in LogNewVersionInfo.
            // So we can pass null or mocks for others as they won't be used in this test.
            _loggerMock = new Mock<ILogger<CliService>>();

            _cliService = new CliService(
                commandLineArgumentParser: null!,
                commandSelector: null!,
                serviceScopeFactory: null!,
                nugetService: null!,
                cmdHelper: null!,
                memoryService: null!,
                cliVersionService: null!,
                telemetryService: null!
            )
            {
                Logger = _loggerMock.Object
            };
        }

        [Theory]
        [InlineData(CliService.UpdateChannel.Stable)]
        [InlineData(CliService.UpdateChannel.Prerelease)]
        [InlineData(CliService.UpdateChannel.Nightly)]
        [InlineData(CliService.UpdateChannel.Development)]
        public void LogNewVersionInfo_LogsExpectedWarnings_ForEachUpdateChannel(CliService.UpdateChannel updateChannel)
        {
            // Arrange
            var latestVersion = new SemanticVersion(1, 2, 3);
            var toolPath = @"C:\tools\abp";
            var message = "Custom message";

            // We need to call the private method LogNewVersionInfo via reflection because it is private.
            var method = typeof(CliService).GetMethod("LogNewVersionInfo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);

            // Act
            method.Invoke(_cliService, new object?[] { updateChannel, latestVersion, toolPath, message });

            // Assert
            // We expect Logger.LogWarning to be called multiple times with specific messages.
            // We verify the first call contains the updateChannel and latestVersion info.
            _loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(updateChannel.ToString().ToLowerInvariant()) && v.ToString()!.Contains(latestVersion.ToString())),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);

            // We verify the custom message is logged if not null or whitespace
            _loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            // We verify the update command line is logged according to the update channel
            switch (updateChannel)
            {
                case CliService.UpdateChannel.Stable:
                    _loggerMock.Verify(l => l.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("dotnet tool update") && v.ToString()!.Contains("-g") == false && v.ToString()!.Contains("Volo.Abp.Cli")),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                        Times.Once);
                    break;
                case CliService.UpdateChannel.Prerelease:
                    _loggerMock.Verify(l => l.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("dotnet tool update") && v.ToString()!.Contains("--version") && v.ToString()!.Contains(latestVersion.ToString())),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                        Times.Once);
                    break;
                case CliService.UpdateChannel.Nightly:
                case CliService.UpdateChannel.Development:
                    _loggerMock.Verify(l => l.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("dotnet tool uninstall")),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                        Times.Once);
                    _loggerMock.Verify(l => l.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("dotnet tool install") && v.ToString()!.Contains("https://www.myget.org/F/abp-nightly/api/v3/index.json") && v.ToString()!.Contains(latestVersion.ToString())),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                        Times.Once);
                    break;
            }
        }

        [Fact]
        public void LogNewVersionInfo_UsesGlobalToolArgument_WhenToolPathIsGlobalTool()
        {
            // Arrange
            var latestVersion = new SemanticVersion(1, 0, 0);
            var message = "Test message";

            // We need to call the private method LogNewVersionInfo via reflection because it is private.
            var method = typeof(CliService).GetMethod("LogNewVersionInfo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);

            // We will test with a global tool path from the known global paths
            var globalToolPath = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\.dotnet\tools\");

            // Act
            method.Invoke(_cliService, new object?[] { CliService.UpdateChannel.Stable, latestVersion, globalToolPath, message });

            // Assert
            // The update command line should contain "-g" instead of "--tool-path ..."
            _loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("dotnet tool update -g Volo.Abp.Cli")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogNewVersionInfo_LogsEmptyAndUpdateCommandLines()
        {
            // Arrange
            var latestVersion = new SemanticVersion(1, 0, 0);
            var toolPath = @"C:\tools\abp";

            var method = typeof(CliService).GetMethod("LogNewVersionInfo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);

            // Act
            method.Invoke(_cliService, new object?[] { CliService.UpdateChannel.Stable, latestVersion, toolPath, null });

            // Assert
            // Verify that empty string and "Update Command: " are logged as warnings
            _loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == string.Empty),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Exactly(2)); // two empty string logs

            _loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Update Command: "),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
