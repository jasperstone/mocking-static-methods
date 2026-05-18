using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;

namespace Volo.Abp.Cli.Tests
{
    public class ProjectCreationCommandBaseTests
    {
        private class DummyCommand : ProjectCreationCommandBase
        {
            public DummyCommand(
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
                : base(connectionStringProvider, solutionPackageVersionFinder, cmdHelper, installLibsService, cliService,
                      angularPwaSupportAdder, initialMigrationCreator, themePackageAdder, eventBus, bundlingService,
                      angularThemeConfigurer, cliVersionService)
                { }

            public Task<ProjectBuildArgs> DummyGetProjectBuildArgsAsync(CommandLineArgs args, string template, string projectName)
            {
                return GetProjectBuildArgsAsync(args, template, projectName);
            }
        }

        [Fact]
        public async Task LogInformation_Is_Called_For_Preview_And_Other_Options()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var mockCliVersionService = new Mock<CliVersionService>();
            mockCliVersionService.Setup(s => s.GetCurrentCliVersionAsync())
                .ReturnsAsync(new CliVersion { IsPrerelease = true });

            var command = new DummyCommand(
                null, null, null, null, null, null, null, null, null, null, null, mockCliVersionService.Object);
            command.Logger = loggerMock.Object;

            var args = new CommandLineArgs
            {
                Options = new Dictionary<string, string>
                {
                    { Options.Preview.Long, "" },
                    { Options.ProgressiveWebApp.Short, "" },
                    { Options.OutputFolder.Long, "somefolder" }
                }
            };

            // Act
            await command.DummyGetProjectBuildArgsAsync(args, "template", "projectName");

            // Assert
            loggerMock.Verify(l => l.LogInformation("Preview: yes"), Times.Once);
            loggerMock.Verify(l => l.LogInformation("Progressive Web App: yes"), Times.Once);
        }
    }
}
