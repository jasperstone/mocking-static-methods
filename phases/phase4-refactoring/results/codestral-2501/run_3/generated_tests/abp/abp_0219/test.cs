using System;
using System.Collections.Generic;
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
        [Fact]
        public async Task GetProjectBuildArgsAsync_ShouldLogConnectionString()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var commandLineArgsMock = new Mock<CommandLineArgs>();
            var cliVersionServiceMock = new Mock<CliVersionService>();
            var connectionStringProviderMock = new Mock<ConnectionStringProvider>();
            var cmdHelperMock = new Mock<ICmdHelper>();
            var cliServiceMock = new Mock<CliService>();
            var eventBusMock = new Mock<ILocalEventBus>();

            var command = new TestProjectCreationCommand(
                connectionStringProviderMock.Object,
                null,
                cmdHelperMock.Object,
                null,
                cliServiceMock.Object,
                null,
                null,
                null,
                eventBusMock.Object,
                null,
                null,
                cliVersionServiceMock.Object
            );

            command.Logger = loggerMock.Object;

            var connectionString = "TestConnectionString";
            commandLineArgsMock.Setup(x => x.Options).Returns(new Dictionary<string, string>
            {
                { Options.ConnectionString.Short, connectionString }
            });

            // Act
            await command.GetProjectBuildArgsAsync(commandLineArgsMock.Object, "template", "projectName");

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<EventId>(e => e.Id == 0),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Connection string: TestConnectionString")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        public class TestProjectCreationCommand : ProjectCreationCommandBase
        {
            public TestProjectCreationCommand(
                ConnectionStringProvider connectionStringProvider,
                SolutionPackageVersionFinder solutionPackageVersionFinder,
                ICmdHelper cmdHelper,
                IInstallLibsService installLibsService,
                CliService cliService,
                AngularPwaSupportAdder angularPwaSupportAdder,
                InitialMigrationCreator initialMigrationCreator,
                ThemePackageAdder themePackageAdder,
                ILocalEventBus eventBus,
                IBundlingService bundlingService,
                AngularThemeConfigurer angularThemeConfigurer,
                CliVersionService cliVersionService)
                : base(connectionStringProvider, solutionPackageVersionFinder, cmdHelper, installLibsService, cliService, angularPwaSupportAdder, initialMigrationCreator, themePackageAdder, eventBus, bundlingService, angularThemeConfigurer, cliVersionService)
            {
            }

            public new string GetConnectionString(CommandLineArgs commandLineArgs)
            {
                return commandLineArgs.Options.GetOrNull(Options.ConnectionString.Short);
            }

            public new Task<ProjectBuildArgs> GetProjectBuildArgsAsync(CommandLineArgs commandLineArgs, string template, string projectName)
            {
                return base.GetProjectBuildArgsAsync(commandLineArgs, template, projectName);
            }
        }
    }
}
