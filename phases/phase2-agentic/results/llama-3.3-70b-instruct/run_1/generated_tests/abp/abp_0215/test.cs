using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class ProjectCreationCommandBaseTests
    {
        [Fact]
        public async Task GetProjectBuildArgsAsync_LogsVersion_WhenVersionIsSpecified()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var commandLineArgs = new CommandLineArgs();
            commandLineArgs.Options.Add(Options.Version.Short, "1.0.0");
            var projectCreationCommandBase = new TestProjectCreationCommandBase(loggerMock.Object);

            // Act
            await projectCreationCommandBase.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            loggerMock.Verify(l => l.LogInformation("Version: 1.0.0"), Times.Once);
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_LogsPreview_WhenPreviewIsSpecified()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var commandLineArgs = new CommandLineArgs();
            commandLineArgs.Options.Add(Options.Preview.Long, true);
            var projectCreationCommandBase = new TestProjectCreationCommandBase(loggerMock.Object);

            // Act
            await projectCreationCommandBase.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            loggerMock.Verify(l => l.LogInformation("Preview: yes"), Times.Once);
        }

        private class TestProjectCreationCommandBase : ProjectCreationCommandBase
        {
            public TestProjectCreationCommandBase(ILogger<NewCommand> logger) : base(
                new ConnectionStringProvider(),
                new SolutionPackageVersionFinder(),
                new CmdHelper(),
                new InstallLibsService(),
                new CliService(),
                new AngularPwaSupportAdder(),
                new InitialMigrationCreator(),
                new ThemePackageAdder(),
                new LocalEventBus(),
                new BundlingService(),
                new AngularThemeConfigurer(),
                new CliVersionService(),
                logger)
            {
            }
        }
    }
}
