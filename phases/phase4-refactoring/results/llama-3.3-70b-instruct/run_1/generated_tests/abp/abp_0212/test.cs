using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class NewCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogInformationCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var templateProjectBuilderMock = new Mock<TemplateProjectBuilder>();
            var telemetryServiceMock = new Mock<ITelemetryService>();
            var newCommand = new NewCommand(
                new ConnectionStringProvider(),
                new SolutionPackageVersionFinder(),
                new CmdHelper(),
                new InstallLibsService(),
                new CliService(
                    new CommandLineArgumentParser(),
                    new CommandSelector(),
                    new ServiceScopeFactory(),
                    new PackageVersionCheckerService(),
                    new CmdHelper(),
                    new MemoryService(),
                    new CliVersionService(),
                    new TelemetryService()
                ),
                new AngularPwaSupportAdder(),
                new InitialMigrationCreator(),
                new ThemePackageAdder(),
                new LocalEventBus(),
                new BundlingService(),
                new TemplateInfoProvider(),
                templateProjectBuilderMock.Object,
                new AngularThemeConfigurer(),
                new CliVersionService(),
                telemetryServiceMock.Object
            );

            // Act
            await newCommand.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
