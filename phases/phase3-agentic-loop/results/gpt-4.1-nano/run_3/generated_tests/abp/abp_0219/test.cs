using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;

namespace Volo.Abp.Cli.Tests
{
    public class ProjectCreationCommandBaseTests
    {
        private class TestCommand : ProjectCreationCommandBase
        {
            public TestCommand(
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
            {
            }

            public Task<ProjectBuildArgs> DummyGetProjectBuildArgsAsync() => Task.FromResult<ProjectBuildArgs>(null);
        }

        [Fact]
        public async Task LogInformation_IsCalled_ForPreviewOption()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var command = new TestCommand(
                null, null, null, null, null, null, null, null, null, null, null, null);
            command.Logger = loggerMock.Object;

            var commandLineArgs = new CommandLineArgs
            {
                Options = new Dictionary<string, string>
                {
                    { Options.Preview.Long, "true" }
                }
            };

            var mockCliVersionService = new Mock<CliVersionService>();
            mockCliVersionService.Setup(s => s.GetCurrentCliVersionAsync())
                .ReturnsAsync(new CliVersion { IsPrerelease = true });

            // Act
            await command.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Preview: yes")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
