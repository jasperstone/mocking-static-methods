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
using Volo.Abp.Cli.ProjectBuilding.Building;

namespace Volo.Abp.Cli.Tests.Commands
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
                .Returns(Task.FromResult<TemplateInfo>(new FakeTemplateInfo("app")));

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

            // Inject the logger mock into the base class Logger property via reflection
            var loggerProperty = typeof(NewCommand).BaseType.GetProperty("Logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerProperty.SetValue(newCommand, loggerMock.Object);

            var options = new AbpCommandLineOptions();
            options.Add("--tiered", "");
            var commandLineArgs = new CommandLineArgs(null, "MyProject");
            // Use reflection to set the readonly Options property
            var optionsProperty = typeof(CommandLineArgs).GetProperty("Options");
            optionsProperty.SetValue(commandLineArgs, options);

            // Setup TemplateProjectBuilder.BuildAsync to return a dummy result
            templateProjectBuilderMock.Setup(t => t.BuildAsync(It.IsAny<ProjectBuildArgs>()))
                .Returns(Task.FromResult(new ProjectBuildResult()));

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

        private class FakeTemplateInfo : TemplateInfo
        {
            public FakeTemplateInfo(string name) : base(name)
            {
            }
        }
    }
}
