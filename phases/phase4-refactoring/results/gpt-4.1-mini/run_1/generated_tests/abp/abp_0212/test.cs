using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Building;
using Volo.Abp.Internal.Telemetry;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class NewCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsInformationIncludingTiered()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var telemetryServiceMock = new Mock<ITelemetryService>();
            var templateProjectBuilderMock = new Mock<TemplateProjectBuilder>(
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<object>(), It.IsAny<object>(), It.IsAny<object>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<object>(), It.IsAny<object>(), It.IsAny<object>(),
                It.IsAny<object>(), It.IsAny<object>());
            var templateInfoProviderMock = new Mock<ITemplateInfoProvider>();

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
                telemetryService: telemetryServiceMock.Object
            );

            // Inject the logger via reflection since Logger is protected in base class
            var loggerProperty = typeof(NewCommand).BaseType.GetProperty("Logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerProperty.SetValue(newCommand, loggerMock.Object);

            var options = new AbpCommandLineOptions
            {
                { "--tiered", "true" }
            };
            var commandLineArgs = new CommandLineArgs(null, "MyProject");
            foreach (var kvp in options)
            {
                commandLineArgs.Options.Add(kvp.Key, kvp.Value);
            }

            templateInfoProviderMock.Setup(t => t.GetDefaultAsync())
                .ReturnsAsync(new FakeTemplateInfo("app"));

            templateProjectBuilderMock.Setup(t => t.BuildAsync(It.IsAny<ProjectBuildArgs>()))
                .ReturnsAsync(new ProjectBuildResult());

            telemetryServiceMock.Setup(t => t.AddActivityAsync(It.IsAny<string>(), It.IsAny<Action<IDictionary<string, object>>>()))
                .Returns(Task.CompletedTask);

            // Act
            await newCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Creating your project...")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Project name: MyProject")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Tiered: yes")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("'MyProject' has been successfully created")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        private class FakeTemplateInfo : TemplateInfo
        {
            public FakeTemplateInfo(string name) : base(name)
            {
            }
        }
    }
}
