using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NuGet.Versioning;
using Volo.Abp.Cli;
using Volo.Abp.Cli.Version;
using Volo.Abp.Cli.Memory;
using Volo.Abp.Cli.Args;
using Volo.Abp.Internal.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class CliService_LogWarningTests
    {
        [Fact]
        public async Task RunAsync_LogsWarningForNewVersion()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CliService>>();
            var packageVersionCheckerMock = new Mock<PackageVersionCheckerService>(null, null);
            var memoryServiceMock = new Mock<MemoryService>();
            var cliVersionServiceMock = new Mock<CliVersionService>(null, null);
            var telemetryServiceMock = new Mock<ITelemetryService>();
            var commandLineArgumentParserMock = new Mock<ICommandLineArgumentParser>();
            var commandSelectorMock = new Mock<ICommandSelector>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var cmdHelperMock = new Mock<ICmdHelper>();

            // Setup memoryService to simulate expired version check
            memoryServiceMock.Setup(m => m.GetAsync(It.IsAny<string>())).ReturnsAsync(System.DateTime.UtcNow.AddDays(-2).ToString("o"));

            // Setup PackageVersionCheckerService to return a newer stable version
            var latestVersion = new SemanticVersion(1, 2, 3);
            var latestVersionInfo = new Volo.Abp.Cli.Version.LatestVersionInfo(latestVersion, "Update available");
            packageVersionCheckerMock.Setup(p => p.GetLatestStableVersionFromGithubAsync())
                .ReturnsAsync(latestVersionInfo);

            // Setup CliVersionService to return current version lower than latest
            cliVersionServiceMock.Setup(c => c.GetCurrentCliVersionAsync())
                .ReturnsAsync(new SemanticVersion(1, 0, 0));

            // Setup CommandLineArgumentParser to parse args to empty command line args
            commandLineArgumentParserMock.Setup(p => p.Parse(It.IsAny<string[]>()))
                .Returns(new CommandLineArgs());

            // Setup CommandSelector to return a dummy command type
            commandSelectorMock.Setup(c => c.Select(It.IsAny<CommandLineArgs>()))
                .Returns(typeof(DummyCommand));

            // Setup IServiceScopeFactory and IServiceScope to provide dummy command
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(DummyCommand)))
                .Returns(new DummyCommand());

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

            serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScopeMock.Object);

            var cliService = new CliService(
                commandLineArgumentParserMock.Object,
                commandSelectorMock.Object,
                serviceScopeFactoryMock.Object,
                packageVersionCheckerMock.Object,
                cmdHelperMock.Object,
                memoryServiceMock.Object,
                cliVersionServiceMock.Object,
                telemetryServiceMock.Object);

            cliService.Logger = loggerMock.Object;

            // Act
            await cliService.RunAsync(new string[0]);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("dotnet tool update")),
                null,
                It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Moq.Times.AtLeastOnce);
        }

        private class DummyCommand : IConsoleCommand
        {
            public Task ExecuteAsync(CommandLineArgs args)
            {
                return Task.CompletedTask;
            }
        }
    }
}
