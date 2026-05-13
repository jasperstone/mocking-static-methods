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
            // Setup dependencies with minimal mocks or nulls as they are not used in the tested method
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
        [InlineData(CliService.UpdateChannel.Stable, "-g", "1.2.3", true)]
        [InlineData(CliService.UpdateChannel.Stable, "--tool-path C:\\tools", "1.2.3", false)]
        [InlineData(CliService.UpdateChannel.Prerelease, "-g", "1.2.3-beta", true)]
        [InlineData(CliService.UpdateChannel.Nightly, "-g", "1.2.3-nightly", true)]
        [InlineData(CliService.UpdateChannel.Development, "-g", "1.2.3-dev", true)]
        public void LogNewVersionInfo_LogsExpectedWarnings(
            CliService.UpdateChannel updateChannel,
            string expectedToolPathArg,
            string versionString,
            bool isGlobalTool)
        {
            // Arrange
            var latestVersion = SemanticVersion.Parse(versionString);
            var toolPath = isGlobalTool ? Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\.dotnet\tools\") : @"C:\tools";
            var message = "Custom message";

            // Act
            // Use reflection to invoke private method LogNewVersionInfo
            var method = typeof(CliService).GetMethod("LogNewVersionInfo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);
            method.Invoke(_cliService, new object?[] { updateChannel, latestVersion, toolPath, message });

            // Assert
            // Verify the first warning log about new version info
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"a newer {updateChannel.ToString().ToLowerInvariant()} version of the abp cli is available: {latestVersion}.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            // Verify the custom message logged if not null or whitespace
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            // Verify the empty string log and "Update Command: " log
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == string.Empty),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeast(2)); // at least two empty string logs

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Update Command: "),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            // Verify the update command log depending on updateChannel
            switch (updateChannel)
            {
                case CliService.UpdateChannel.Stable:
                    _loggerMock.Verify(
                        x => x.Log(
                            LogLevel.Warning,
                            It.IsAny<EventId>(),
                            It.Is<It.IsAnyType>((v, t) => v.ToString() == $"dotnet tool update {expectedToolPathArg} Volo.Abp.Cli"),
                            null,
                            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                        Times.Once);
                    break;

                case CliService.UpdateChannel.Prerelease:
                    _loggerMock.Verify(
                        x => x.Log(
                            LogLevel.Warning,
                            It.IsAny<EventId>(),
                            It.Is<It.IsAnyType>((v, t) => v.ToString() == $"dotnet tool update {expectedToolPathArg} Volo.Abp.Cli --version {latestVersion}"),
                            null,
                            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                        Times.Once);
                    break;

                case CliService.UpdateChannel.Nightly:
                case CliService.UpdateChannel.Development:
                    _loggerMock.Verify(
                        x => x.Log(
                            LogLevel.Warning,
                            It.IsAny<EventId>(),
                            It.Is<It.IsAnyType>((v, t) => v.ToString() == $"dotnet tool uninstall {expectedToolPathArg} Volo.Abp.Cli"),
                            null,
                            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                        Times.Once);

                    _loggerMock.Verify(
                        x => x.Log(
                            LogLevel.Warning,
                            It.IsAny<EventId>(),
                            It.Is<It.IsAnyType>((v, t) => v.ToString()!.StartsWith($"dotnet tool install {expectedToolPathArg} Volo.Abp.Cli --add-source https://www.myget.org/F/abp-nightly/api/v3/index.json --version")),
                            null,
                            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                        Times.Once);
                    break;
            }

            // Verify the final empty string log
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == string.Empty),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeast(2));
        }

        [Fact]
        public void LogNewVersionInfo_LogsWarningWithoutMessage()
        {
            // Arrange
            var latestVersion = SemanticVersion.Parse("1.0.0");
            var toolPath = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\.dotnet\tools\");
            string? message = null;

            // Act
            var method = typeof(CliService).GetMethod("LogNewVersionInfo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);
            method.Invoke(_cliService, new object?[] { CliService.UpdateChannel.Stable, latestVersion, toolPath, message });

            // Assert
            // The message log should not be called
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == null || !v.ToString()!.Contains("Custom message")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);

            // The warning about new version should be logged once
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("a newer stable version of the abp cli is available: 1.0.0.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
