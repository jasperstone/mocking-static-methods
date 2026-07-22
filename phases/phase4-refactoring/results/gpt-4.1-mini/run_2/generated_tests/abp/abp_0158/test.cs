using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NuGet.Versioning;
using Volo.Abp.Cli;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Memory;
using Volo.Abp.Cli.Version;
using Volo.Abp.Internal.Telemetry;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class CliServiceTests
    {
        private readonly Mock<ILogger<CliService>> _loggerMock;
        private readonly Mock<ICommandLineArgumentParser> _commandLineArgumentParserMock;
        private readonly Mock<ICommandSelector> _commandSelectorMock;
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<PackageVersionCheckerService> _packageVersionCheckerServiceMock;
        private readonly Mock<ICmdHelper> _cmdHelperMock;
        private readonly Mock<MemoryService> _memoryServiceMock;
        private readonly Mock<CliVersionService> _cliVersionServiceMock;
        private readonly Mock<ITelemetryService> _telemetryServiceMock;

        public CliServiceTests()
        {
            _loggerMock = new Mock<ILogger<CliService>>();
            _commandLineArgumentParserMock = new Mock<ICommandLineArgumentParser>();
            _commandSelectorMock = new Mock<ICommandSelector>();
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>(null, null);
            _cmdHelperMock = new Mock<ICmdHelper>();
            _memoryServiceMock = new Mock<MemoryService>(null);
            _cliVersionServiceMock = new Mock<CliVersionService>(null, null);
            _telemetryServiceMock = new Mock<ITelemetryService>();
        }

        [Fact]
        public async Task RunAsync_LogsWarningForNewerStableVersion()
        {
            // Arrange
            var currentVersion = new SemanticVersion(1, 0, 0);
            var latestVersion = new SemanticVersion(1, 1, 0);
            var latestVersionInfo = new LatestVersionInfo
            {
                Version = latestVersion,
                Message = "Test message"
            };

            _cliVersionServiceMock.Setup(c => c.GetCurrentCliVersionAsync()).ReturnsAsync(currentVersion);
            _commandLineArgumentParserMock.Setup(p => p.Parse(It.IsAny<string[]>())).Returns(new CommandLineArgs("any"));
            _memoryServiceMock.Setup(m => m.GetAsync(It.IsAny<string>())).ReturnsAsync((string)null);
            _packageVersionCheckerServiceMock.Setup(p => p.GetLatestStableVersionFromGithubAsync()).ReturnsAsync(latestVersionInfo);

            var cliService = new TestCliService(
                _commandLineArgumentParserMock.Object,
                _commandSelectorMock.Object,
                _serviceScopeFactoryMock.Object,
                _packageVersionCheckerServiceMock.Object,
                _cmdHelperMock.Object,
                _memoryServiceMock.Object,
                _cliVersionServiceMock.Object,
                _telemetryServiceMock.Object
            )
            {
                Logger = _loggerMock.Object,
                LatestVersionInfoToReturn = latestVersionInfo,
                CurrentVersionToReturn = currentVersion,
                ToolPathToReturn = "-g",
                UpdateChannelToReturn = 1 // Stable enum value
            };

            // Act
            await cliService.RunAsync(new string[] { });

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("A newer stable version of the ABP CLI is available")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Test message")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("dotnet tool update -g Volo.Abp.Cli")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        private class TestCliService : CliService
        {
            public LatestVersionInfo LatestVersionInfoToReturn { get; set; }
            public SemanticVersion CurrentVersionToReturn { get; set; }
            public int UpdateChannelToReturn { get; set; }
            public string ToolPathToReturn { get; set; }

            public TestCliService(
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

            protected override string GetToolPath(Assembly assembly)
            {
                return ToolPathToReturn;
            }

            protected override object GetUpdateChannel(SemanticVersion currentCliVersion)
            {
                // Return boxed int as enum since enum is protected
                return Enum.ToObject(typeof(object).Assembly.GetType("Volo.Abp.Cli.CliService+UpdateChannel") ?? typeof(object), UpdateChannelToReturn);
            }

            protected override Task<LatestVersionInfo> GetLatestVersion(object updateChannel)
            {
                return Task.FromResult(LatestVersionInfoToReturn);
            }
        }
    }

    // Minimal stub classes to satisfy references
    public class LatestVersionInfo
    {
        public SemanticVersion Version { get; set; }
        public string Message { get; set; }
    }
}
