using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
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

namespace Volo.Abp.Cli.Commands.Tests
{
    public class ProjectCreationCommandBaseTests
    {
        private readonly Mock<ILogger<NewCommand>> _loggerMock;
        private readonly ProjectCreationCommandBase _commandBase;

        public ProjectCreationCommandBaseTests()
        {
            _loggerMock = new Mock<ILogger<NewCommand>>();
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
                Mock.Of<CliVersionService>(),
                Mock.Of<ITelemetryService>())
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_ShouldLogPreview()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs(new[] { "--preview" });

            // Act
            await _commandBase.GetProjectBuildArgsAsync(commandLineArgs, "app", "TestProject");

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Preview: yes"),
                Times.Once);
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_ShouldLogProgressiveWebApp()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs(new[] { "--pwa" });

            // Act
            await _commandBase.GetProjectBuildArgsAsync(commandLineArgs, "app", "TestProject");

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Progressive Web App: yes"),
                Times.Once);
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_ShouldLogDatabaseProvider()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs(new[] { "--database-provider", "SqlServer" });

            // Act
            await _commandBase.GetProjectBuildArgsAsync(commandLineArgs, "app", "TestProject");

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Database provider: SqlServer"),
                Times.Once);
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_ShouldLogConnectionString()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs(new[] { "--connection-string", "TestConnectionString" });

            // Act
            await _commandBase.GetProjectBuildArgsAsync(commandLineArgs, "app", "TestProject");

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Connection string: TestConnectionString"),
                Times.Once);
        }
    }
}
