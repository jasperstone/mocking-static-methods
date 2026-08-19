using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.ProjectBuilding;
using Xunit;

namespace Volo.Abp.Cli.Tests;

public class ProjectCreationCommandBaseTests
{
    [Fact]
    public async Task GetProjectBuildArgsAsync_Version_LogsVersion()
    {
        // Arrange
        var commandLineArgs = new CommandLineArgs();
        commandLineArgs.Options.Add(Volo.Abp.Cli.Commands.Options.Version.Short, "1.0.0");
        var loggerMock = new Mock<ILogger<NewCommand>>();
        var projectCreationCommandBase = new ProjectCreationCommandBaseMock(loggerMock.Object);

        // Act
        await projectCreationCommandBase.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

        // Assert
        loggerMock.Verify(l => l.LogInformation("Version: 1.0.0"), Times.Once);
    }

    [Fact]
    public async Task GetProjectBuildArgsAsync_Preview_LogsPreview()
    {
        // Arrange
        var commandLineArgs = new CommandLineArgs();
        commandLineArgs.Options.Add(Volo.Abp.Cli.Commands.Options.Preview.Long, null);
        var loggerMock = new Mock<ILogger<NewCommand>>();
        var projectCreationCommandBase = new ProjectCreationCommandBaseMock(loggerMock.Object);

        // Act
        await projectCreationCommandBase.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

        // Assert
        loggerMock.Verify(l => l.LogInformation("Preview: yes"), Times.Once);
    }

    [Fact]
    public async Task GetProjectBuildArgsAsync_ProgressiveWebApp_LogsProgressiveWebApp()
    {
        // Arrange
        var commandLineArgs = new CommandLineArgs();
        commandLineArgs.Options.Add(Volo.Abp.Cli.Commands.Options.ProgressiveWebApp.Short, null);
        var loggerMock = new Mock<ILogger<NewCommand>>();
        var projectCreationCommandBase = new ProjectCreationCommandBaseMock(loggerMock.Object);

        // Act
        await projectCreationCommandBase.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

        // Assert
        loggerMock.Verify(l => l.LogInformation("Progressive Web App: yes"), Times.Once);
    }

    private class ProjectCreationCommandBaseMock : ProjectCreationCommandBase
    {
        public ProjectCreationCommandBaseMock(ILogger<NewCommand> logger)
            : base(
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
                new CliVersionService())
        {
            Logger = logger;
        }

        public new Task<ProjectBuildArgs> GetProjectBuildArgsAsync(CommandLineArgs commandLineArgs, string template, string projectName)
        {
            return base.GetProjectBuildArgsAsync(commandLineArgs, template, projectName);
        }
    }
}
