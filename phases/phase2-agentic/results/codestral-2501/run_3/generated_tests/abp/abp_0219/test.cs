using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Templates;
using Volo.Abp.Cli.ProjectBuilding.Templates.App;
using Volo.Abp.Cli.ProjectBuilding.Templates.Microservice;
using Volo.Abp.Cli.ProjectBuilding.Templates.Module;
using Volo.Abp.Cli.ProjectBuilding.Templates.MvcModule;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Cli.Version;
using Volo.Abp.EventBus.Local;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class ProjectCreationCommandBaseTests
    {
        private readonly Mock<ILogger<NewCommand>> _loggerMock;
        private readonly Mock<CliVersionService> _cliVersionServiceMock;
        private readonly ProjectCreationCommandBase _commandBase;

        public ProjectCreationCommandBaseTests()
        {
            _loggerMock = new Mock<ILogger<NewCommand>>();
            _cliVersionServiceMock = new Mock<CliVersionService>();

            _commandBase = new NewCommand(
                Mock.Of<ConnectionStringProvider>(),
                Mock.Of<SolutionPackageVersionFinder>(),
                Mock.Of<ICmdHelper>(),
                Mock.Of<IInstallLibsService>(),
                Mock.Of<CliService>(),
                Mock.Of<AngularPwaSupportAdder>(),
                Mock.Of<InitialMigrationCreator>(),
                Mock.Of<ThemePackageAdder>(),
                Mock.Of<ILocalEventBus>(),
                Mock.Of<IBundlingService>(),
                Mock.Of<ITemplateInfoProvider>(),
                Mock.Of<TemplateProjectBuilder>(),
                Mock.Of<AngularThemeConfigurer>(),
                _cliVersionServiceMock.Object,
                Mock.Of<ITelemetryService>()
            )
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_ShouldLogConnectionString()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs
            {
                Options = new AbpCommandLineOptions
                {
                    { "connection-string", "TestConnectionString" }
                }
            };

            // Act
            await _commandBase.GetProjectBuildArgsAsync(commandLineArgs, "app", "TestProject");

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Connection string: {ConnectionString}", "TestConnectionString"),
                Times.Once);
        }
    }
}
