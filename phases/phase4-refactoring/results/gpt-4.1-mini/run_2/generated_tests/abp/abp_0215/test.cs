using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Version;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class ProjectCreationCommandBaseTests
    {
        private class FakeCliVersionService : CliVersionService
        {
            public override Task<CliVersion> GetCurrentCliVersionAsync()
            {
                return Task.FromResult(new CliVersion("1.0.0-preview", true));
            }
        }

        private class TestProjectCreationCommand : ProjectCreationCommandBase
        {
            public TestProjectCreationCommand(CliVersionService cliVersionService)
                : base(
                    connectionStringProvider: null,
                    solutionPackageVersionFinder: null,
                    cmdHelper: null,
                    installLibsService: null,
                    cliService: null,
                    angularPwaSupportAdder: null,
                    initialMigrationCreator: null,
                    themePackageAdder: null,
                    eventBus: null,
                    bundlingService: null,
                    angularThemeConfigurer: null,
                    cliVersionService: cliVersionService)
            {
            }

            public new async Task<Volo.Abp.Cli.ProjectBuilding.ProjectBuildArgs> GetProjectBuildArgsAsync(CommandLineArgs commandLineArgs, string template, string projectName)
            {
                return await base.GetProjectBuildArgsAsync(commandLineArgs, template, projectName);
            }
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_LogsVersionInformation_WhenVersionOptionIsPresent()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var cliVersionService = new FakeCliVersionService();
            var command = new TestProjectCreationCommand(cliVersionService);
            command.Logger = loggerMock.Object;

            var options = new Dictionary<string, string>
            {
                { "v", "1.2.3" }
            };
            var commandLineArgs = new CommandLineArgs();
            foreach (var kvp in options)
            {
                commandLineArgs.Options[kvp.Key] = kvp.Value;
            }

            // Act
            await command.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Version: 1.2.3")),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);
        }
    }
}
