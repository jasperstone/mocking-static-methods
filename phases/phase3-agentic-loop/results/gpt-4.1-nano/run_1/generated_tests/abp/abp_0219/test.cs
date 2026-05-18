using System;
using System.Collections.Generic;
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
            {
            }

            // Expose protected method for testing
            public async Task TestLogInformationAsync(CommandLineArgs args, string template, string projectName)
            {
                await GetProjectBuildArgsAsync(args, template, projectName);
            }

            // Override abstract methods with dummy implementations
            protected override DatabaseProvider GetDatabaseProvider(CommandLineArgs args) => DatabaseProvider.NotSpecified;
            protected override string GetConnectionString(CommandLineArgs args) => null;
            protected override DatabaseManagementSystem GetDatabaseManagementSystem(CommandLineArgs args) => DatabaseManagementSystem.NotSpecified;
            protected override UiFramework GetUiFramework(CommandLineArgs args, string template) => UiFramework.NotSpecified;
            protected override MobileApp GetMobilePreference(CommandLineArgs args, string template) => MobileApp.None;
            protected override string GetCreateSolutionFolderPreference(CommandLineArgs args) => null;
        }

        [Fact]
        public async Task LogInformation_Called_For_PreviewOption()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var cliVersionServiceMock = new Mock<CliVersionService>();
            cliVersionServiceMock.Setup(s => s.GetCurrentCliVersionAsync()).ReturnsAsync(new CliVersion { IsPrerelease = true });

            var command = new DummyCommand(
                null, null, null, null, null, null, null, null, null, null, null, cliVersionServiceMock.Object);
            command.Logger = loggerMock.Object;

            var args = new CommandLineArgs
            {
                Options = new Dictionary<string, string>
                {
                    { Options.Preview.Long, null }
                }
            };

            // Act
            await command.TestLogInformationAsync(args, "template", "projectName");

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
