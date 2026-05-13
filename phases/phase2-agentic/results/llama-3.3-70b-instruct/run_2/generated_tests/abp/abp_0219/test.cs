using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class ProjectCreationCommandBaseTests
    {
        [Fact]
        public async Task GetProjectBuildArgsAsync_LogsConnectionString()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var commandLineArgs = new CommandLineArgs();
            commandLineArgs.Options.Add("connection_string", "connection_string");
            var projectCreationCommandBase = new ProjectCreationCommandBase(
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
                loggerMock.Object);

            // Act
            await projectCreationCommandBase.GetProjectBuildArgsAsync(commandLineArgs, "template", "project_name");

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Connection string: connection_string"), Times.Once);
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_LogsDatabaseProvider()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var commandLineArgs = new CommandLineArgs();
            commandLineArgs.Options.Add("database_provider", "database_provider");
            var projectCreationCommandBase = new ProjectCreationCommandBase(
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
                loggerMock.Object);

            // Act
            await projectCreationCommandBase.GetProjectBuildArgsAsync(commandLineArgs, "template", "project_name");

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Database provider: database_provider"), Times.Once);
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_LogsDatabaseManagementSystem()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var commandLineArgs = new CommandLineArgs();
            commandLineArgs.Options.Add("database_management_system", "database_management_system");
            var projectCreationCommandBase = new ProjectCreationCommandBase(
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
                loggerMock.Object);

            // Act
            await projectCreationCommandBase.GetProjectBuildArgsAsync(commandLineArgs, "template", "project_name");

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("DBMS: database_management_system"), Times.Once);
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_LogsUiFramework()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var commandLineArgs = new CommandLineArgs();
            commandLineArgs.Options.Add("ui_framework", "ui_framework");
            var projectCreationCommandBase = new ProjectCreationCommandBase(
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
                loggerMock.Object);

            // Act
            await projectCreationCommandBase.GetProjectBuildArgsAsync(commandLineArgs, "template", "project_name");

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("UI Framework: ui_framework"), Times.Once);
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_LogsPublicWebSite()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var commandLineArgs = new CommandLineArgs();
            commandLineArgs.Options.Add("public_web_site", "");
            var projectCreationCommandBase = new ProjectCreationCommandBase(
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
                loggerMock.Object);

            // Act
            await projectCreationCommandBase.GetProjectBuildArgsAsync(commandLineArgs, "template", "project_name");

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Public Web Site: yes"), Times.Once);
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_LogsMobileApp()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var commandLineArgs = new CommandLineArgs();
            commandLineArgs.Options.Add("mobile_app", "mobile_app");
            var projectCreationCommandBase = new ProjectCreationCommandBase(
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
                loggerMock.Object);

            // Act
            await projectCreationCommandBase.GetProjectBuildArgsAsync(commandLineArgs, "template", "project_name");

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Mobile App: mobile_app"), Times.Once);
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_LogsGitHubAbpLocalRepositoryPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var commandLineArgs = new CommandLineArgs();
            commandLineArgs.Options.Add("github_abp_local_repository_path", "github_abp_local_repository_path");
            var projectCreationCommandBase = new ProjectCreationCommandBase(
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
                loggerMock.Object);

            // Act
            await projectCreationCommandBase.GetProjectBuildArgsAsync(commandLineArgs, "template", "project_name");

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("GitHub Abp Local Repository Path: github_abp_local_repository_path"), Times.Once);
        }
    }
}
