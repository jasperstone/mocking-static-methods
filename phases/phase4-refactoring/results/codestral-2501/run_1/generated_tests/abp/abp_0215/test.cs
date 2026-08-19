using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Bundling;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.LIbs;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Events;
using Volo.Abp.Cli.ProjectBuilding.Templates.Module;
using Volo.Abp.Cli.ProjectModification;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Cli.Version;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Internal.Telemetry;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class ProjectCreationCommandBaseTests
    {
        [Fact]
        public async Task GetProjectBuildArgsAsync_ShouldLogVersion_WhenVersionIsSpecified()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<NewCommand>>();
            var commandLineArgs = new CommandLineArgs(new[] { "--version", "1.0.0" });
            var projectCreationCommandBase = new NewCommand(
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
                Mock.Of<ITelemetryService>()
            )
            {
                Logger = mockLogger.Object
            };

            // Act
            await projectCreationCommandBase.GetProjectBuildArgsAsync(commandLineArgs, "app", "TestProject");

            // Assert
            mockLogger.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Version: 1.0.0")),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>()),
                Times.Once);
        }
    }
}
