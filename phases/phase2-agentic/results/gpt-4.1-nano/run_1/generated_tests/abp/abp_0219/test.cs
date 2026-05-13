using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Moq;
using Xunit;
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

            // Expose protected method for testing
            public new Task<ProjectBuildArgs> GetProjectBuildArgsAsync(CommandLineArgs args, string template, string projectName)
            {
                return base.GetProjectBuildArgsAsync(args, template, projectName);
            }
        }

        [Fact]
        public async Task LogInformation_Is_Called_For_Preview_When_Preview_Is_True()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var mockCliVersionService = new Mock<CliVersionService>();
            mockCliVersionService.Setup(s => s.GetCurrentCliVersionAsync())
                .ReturnsAsync(new CliVersion { IsPrerelease = true });

            var command = new TestCommand(
                null, null, null, null, null, null, null, null, null, null, null, mockCliVersionService.Object);
            command.Logger = loggerMock.Object;

            var args = new CommandLineArgs
            {
                Options = new Dictionary<string, string> { { Options.Preview.Long, "true" } }
            };

            // Act
            await command.GetProjectBuildArgsAsync(args, "template", "projectName");

            // Assert
            loggerMock.Verify(l => l.LogInformation("Preview: yes"), Times.Once);
        }

        [Fact]
        public async Task LogInformation_Is_Called_For_ProgressiveWebApp_When_Pwa_Is_Present()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var command = new TestCommand(
                null, null, null, null, null, null, null, null, null, null, null, null);
            command.Logger = loggerMock.Object;

            var args = new CommandLineArgs
            {
                Options = new Dictionary<string, string> { { Options.ProgressiveWebApp.Short, "true" } }
            };

            // Act
            await command.GetProjectBuildArgsAsync(args, "template", "projectName");

            // Assert
            loggerMock.Verify(l => l.LogInformation("Progressive Web App: yes"), Times.Once);
        }

        [Fact]
        public async Task LogInformation_Is_Called_For_DatabaseProvider_When_DatabaseProvider_Is_Specified()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var command = new TestCommand(
                null, null, null, null, null, null, null, null, null, null, null, null);
            command.Logger = loggerMock.Object;

            var args = new CommandLineArgs
            {
                Options = new Dictionary<string, string> { { "database", "SqlServer" } }
            };

            // Mock GetDatabaseProvider to return DatabaseProvider.SqlServer
            // Since method is not accessible, assume it returns DatabaseProvider.SqlServer
            // For this test, we simulate the call directly

            // Act
            await command.GetProjectBuildArgsAsync(args, "template", "projectName");

            // Assert
            loggerMock.Verify(l => l.LogInformation("Database provider: SqlServer"), Times.Once);
        }

        [Fact]
        public async Task LogInformation_Is_Called_For_ConnectionString_When_Provided()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var command = new TestCommand(
                null, null, null, null, null, null, null, null, null, null, null, null);
            command.Logger = loggerMock.Object;

            var connectionString = "Server=myServer;Database=myDb;";
            var args = new CommandLineArgs
            {
                Options = new Dictionary<string, string> { { "connectionString", connectionString } }
            };

            // Act
            await command.GetProjectBuildArgsAsync(args, "template", "projectName");

            // Assert
            loggerMock.Verify(l => l.LogInformation($"Connection string: {connectionString}"), Times.Once);
        }
    }

    // Placeholder classes for missing types
    public class CommandLineArgs
    {
        public Dictionary<string, string> Options { get; set; } = new Dictionary<string, string>();
    }

    public class CliVersion
    {
        public bool IsPrerelease { get; set; }
    }

    public static class Options
    {
        public const string Preview = "preview";
        public const string ProgressiveWebApp = "pwa";
        public const string Version = "version";
        public const string PublicWebSite = "publicWebSite";
        public const string GitHubAbpLocalRepositoryPath = "gitHubAbpLocalRepositoryPath";
        public const string GitHubVoloLocalRepositoryPath = "gitHubVoloLocalRepositoryPath";

        public static string Long => "longOption";
        public static string Short => "shortOption";
    }

    public class CliVersionService
    {
        public virtual Task<CliVersion> GetCurrentCliVersionAsync() => Task.FromResult(new CliVersion { IsPrerelease = true });
    }
}
