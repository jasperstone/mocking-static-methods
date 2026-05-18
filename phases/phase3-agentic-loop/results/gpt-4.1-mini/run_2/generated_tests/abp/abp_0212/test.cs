using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Internal.Telemetry;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests
{
    public class NewCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsTieredYes_WhenTieredOptionIsPresent()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var telemetryServiceMock = new Mock<ITelemetryService>();
            var templateProjectBuilderMock = new Mock<TemplateProjectBuilder>(
                MockBehavior.Strict,
                null, null, null, null, null, null, null, null, null, null, null, null, null, null);

            var templateInfoProviderMock = new Mock<ITemplateInfoProvider>();
            templateInfoProviderMock.Setup(t => t.GetDefaultAsync())
                .ReturnsAsync(new TemplateInfo { Name = "app" });

            var newCommand = new NewCommand(
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
                templateInfoProvider: templateInfoProviderMock.Object,
                templateProjectBuilder: templateProjectBuilderMock.Object,
                angularThemeConfigurer: null,
                cliVersionService: null,
                telemetryService: telemetryServiceMock.Object);

            // Inject the logger mock into the base class Logger property via reflection
            var loggerField = typeof(NewCommand).BaseType.GetProperty("Logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(newCommand, loggerMock.Object);

            var options = new Dictionary<string, string>
            {
                { "--tiered", "" }
            };
            var commandLineArgs = new CommandLineArgs("MyProject", options);

            // Setup TemplateProjectBuilder.BuildAsync to return dummy result
            templateProjectBuilderMock.Setup(t => t.BuildAsync(It.IsAny<ProjectBuildArgs>()))
                .ReturnsAsync(new ProjectBuildResult { });

            // Setup other async methods to complete successfully
            telemetryServiceMock.Setup(t => t.AddActivityAsync(It.IsAny<string>(), It.IsAny<Action<IDictionary<string, object>>>()))
                .Returns(Task.CompletedTask);

            // Act
            await newCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Tiered: yes")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Minimal stubs for required types
    public class TemplateInfo
    {
        public string Name { get; set; }
    }

    public class ProjectBuildArgs
    {
        public Dictionary<string, object> ExtraProperties { get; } = new Dictionary<string, object>();
        public string OutputFolder { get; set; } = "output";
    }

    public class ProjectBuildResult
    {
    }
}
