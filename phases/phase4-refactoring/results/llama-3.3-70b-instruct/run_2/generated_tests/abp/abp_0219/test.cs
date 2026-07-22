using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Volo.Abp.Cli.Tests
{
    public class ProjectCreationCommandBaseTests
    {
        [Fact]
        public async Task GetProjectBuildArgsAsync_ValidArgs_ReturnsProjectBuildArgs()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs("MyProject", new Dictionary<string, string>
            {
                { "template", "app" },
                { "ui", "angular" },
                { "mobile", "react" },
                { "database-provider", "sqlserver" },
                { "output-folder", "MyProjectFolder" }
            });

            var loggerMock = new Mock<ILogger>();
            var projectCreationCommandBase = new ProjectCreationCommandBase(
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
                Mock.Of<AngularThemeConfigurer>(),
                Mock.Of<CliVersionService>()
            );
            projectCreationCommandBase.Logger = loggerMock.Object;

            // Act
            var projectBuildArgs = await projectCreationCommandBase.GetProjectBuildArgsAsync(commandLineArgs, "app", "MyProject");

            // Assert
            Assert.NotNull(projectBuildArgs);
            Assert.Equal("MyProject", projectBuildArgs.ProjectName);
            Assert.Equal("app", projectBuildArgs.Template);
            Assert.Equal("angular", projectBuildArgs.UiFramework);
            Assert.Equal("react", projectBuildArgs.MobileApp);
            Assert.Equal("sqlserver", projectBuildArgs.DatabaseProvider);
            Assert.Equal("MyProjectFolder", projectBuildArgs.OutputFolder);
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_InvalidArgs_ThrowsException()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs("MyProject", new Dictionary<string, string>
            {
                { "template", "invalid" },
                { "ui", "angular" },
                { "mobile", "react" },
                { "database-provider", "sqlserver" },
                { "output-folder", "MyProjectFolder" }
            });

            var loggerMock = new Mock<ILogger>();
            var projectCreationCommandBase = new ProjectCreationCommandBase(
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
                Mock.Of<AngularThemeConfigurer>(),
                Mock.Of<CliVersionService>()
            );
            projectCreationCommandBase.Logger = loggerMock.Object;

            // Act and Assert
            await Assert.ThrowsAsync<CliUsageException>(() => projectCreationCommandBase.GetProjectBuildArgsAsync(commandLineArgs, "app", "MyProject"));
        }
    }
}
