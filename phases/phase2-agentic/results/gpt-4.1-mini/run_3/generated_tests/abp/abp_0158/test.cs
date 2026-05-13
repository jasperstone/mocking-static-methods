using System;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli;
using Xunit;
using NuGet.Versioning;

namespace Volo.Abp.Cli.Tests
{
    public class CliService_LogNewVersionInfo_Tests
    {
        private readonly Mock<ILogger<CliService>> _loggerMock;
        private readonly CliService _cliService;

        public CliService_LogNewVersionInfo_Tests()
        {
            // We need to create a CliService instance with mocks for dependencies.
            // Since the constructor requires many dependencies, we will mock them minimally.
            var commandLineArgumentParserMock = new Mock<ICommandLineArgumentParser>();
            var commandSelectorMock = new Mock<ICommandSelector>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>(null, null, null);
            var cmdHelperMock = new Mock<ICmdHelper>();
            var memoryServiceMock = new Mock<MemoryService>(null);
            var cliVersionServiceMock = new Mock<CliVersionService>(null, null, null);
            var telemetryServiceMock = new Mock<ITelemetryService>();

            _cliService = new CliService(
                commandLineArgumentParserMock.Object,
                commandSelectorMock.Object,
                serviceScopeFactoryMock.Object,
                packageVersionCheckerServiceMock.Object,
                cmdHelperMock.Object,
                memoryServiceMock.Object,
                cliVersionServiceMock.Object,
                telemetryServiceMock.Object);

            _loggerMock = new Mock<ILogger<CliService>>();
            _cliService.Logger = _loggerMock.Object;
        }

        [Theory]
        [InlineData(CliService.UpdateChannel.Stable, "-g", "dotnet tool update -g Volo.Abp.Cli")]
        [InlineData(CliService.UpdateChannel.Stable, "--tool-path C:\\tools", "dotnet tool update --tool-path C:\\tools Volo.Abp.Cli")]
        [InlineData(CliService.UpdateChannel.Prerelease, "-g", "dotnet tool update -g Volo.Abp.Cli --version 1.2.3")]
        [InlineData(CliService.UpdateChannel.Nightly, "-g", "dotnet tool uninstall -g Volo.Abp.Cli")]
        [InlineData(CliService.UpdateChannel.Development, "-g", "dotnet tool uninstall -g Volo.Abp.Cli")]
        public void LogNewVersionInfo_LogsExpectedWarnings(CliService.UpdateChannel updateChannel, string toolPath, string expectedFirstCommand)
        {
            // Arrange
            var latestVersion = new SemanticVersion(1, 2, 3);
            var message = "Custom message";

            // We need to override IsGlobalTool to control toolPathArg.
            // Since IsGlobalTool is private, we will use reflection to set a backing field or simulate by calling LogNewVersionInfo with toolPath that matches global tool paths.
            // The global tool paths are "%USERPROFILE%\.dotnet\tools\" and "%HOME%/.dotnet/tools/" expanded.
            // So for toolPath "-g" we simulate global tool by passing one of those paths.
            // For non-global tool, pass a custom path.

            // We will create a derived class to override IsGlobalTool for testing.
            var testCliService = new TestCliService(_loggerMock.Object);

            // Act
            testCliService.InvokeLogNewVersionInfo(updateChannel, latestVersion, toolPath, message);

            // Assert
            // We expect the following LogWarning calls in order:
            // 1. "A newer {updateChannel} version of the ABP CLI is available: {latestVersion}."
            // 2. message (if not null or whitespace)
            // 3. empty string
            // 4. "Update Command: "
            // 5. The update command(s) depending on updateChannel
            // 6. empty string

            // Capture the logged messages
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == $"A newer {updateChannel.ToString().ToLowerInvariant()} version of the ABP CLI is available: {latestVersion}."),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == string.Empty),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2)); // two empty string logs

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Update Command: "),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify the update command(s)
            if (updateChannel == CliService.UpdateChannel.Stable || updateChannel == CliService.UpdateChannel.Prerelease)
            {
                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString() == expectedFirstCommand),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
            else if (updateChannel == CliService.UpdateChannel.Nightly || updateChannel == CliService.UpdateChannel.Development)
            {
                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString() == expectedFirstCommand),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);

                var expectedSecondCommand = $"dotnet tool install {toolPath} Volo.Abp.Cli --add-source https://www.myget.org/F/abp-nightly/api/v3/index.json --version {latestVersion}";
                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString() == expectedSecondCommand),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
        }

        private class TestCliService : CliService
        {
            private readonly ILogger<CliService> _logger;

            public TestCliService(ILogger<CliService> logger) : base(
                new Mock<ICommandLineArgumentParser>().Object,
                new Mock<ICommandSelector>().Object,
                new Mock<IServiceScopeFactory>().Object,
                new Mock<PackageVersionCheckerService>(null, null, null).Object,
                new Mock<ICmdHelper>().Object,
                new Mock<MemoryService>(null).Object,
                new Mock<CliVersionService>(null, null, null).Object,
                new Mock<ITelemetryService>().Object)
            {
                _logger = logger;
                Logger = logger;
            }

            protected override bool IsGlobalTool(string toolPath)
            {
                // For testing, treat "-g" as global tool, anything else as non-global
                return toolPath == "-g";
            }

            public void InvokeLogNewVersionInfo(UpdateChannel updateChannel, SemanticVersion latestVersion, string toolPath, string message = null)
            {
                LogNewVersionInfo(updateChannel, latestVersion, toolPath, message);
            }
        }
    }
}
