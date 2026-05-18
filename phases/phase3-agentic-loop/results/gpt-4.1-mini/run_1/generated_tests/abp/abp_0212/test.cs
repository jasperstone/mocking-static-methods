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
                null, null, null, null, null, null, null, null, null, null, null, null, null, null);
            var templateInfoProviderMock = new Mock<ITemplateInfoProvider>();
            var templateInfo = new Volo.Abp.Cli.ProjectBuilding.TemplateInfo { Name = "app" };
            templateInfoProviderMock.Setup(t => t.GetDefaultAsync())
                .ReturnsAsync(templateInfo);

            var newCommand = new NewCommand(
                null, null, null, null, null, null, null, null, null, null,
                templateInfoProviderMock.Object,
                templateProjectBuilderMock.Object,
                null, null, telemetryServiceMock.Object);

            // Inject the logger mock into the base class Logger property via reflection
            var loggerProperty = typeof(NewCommand).BaseType.GetProperty("Logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerProperty.SetValue(newCommand, loggerMock.Object);

            var commandLineArgs = new CommandLineArgs(null, "MyProject");
            commandLineArgs.Options["tiered"] = "true";

            // Setup TemplateProjectBuilder.BuildAsync to return dummy result
            templateProjectBuilderMock.Setup(t => t.BuildAsync(It.IsAny<ProjectBuildArgs>()))
                .ReturnsAsync(new ProjectBuildResult());

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
}
