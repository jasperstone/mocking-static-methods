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
        public async Task ExecuteAsync_LogsInformationIncludingTiered()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var telemetryServiceMock = new Mock<ITelemetryService>();
            var templateProjectBuilderMock = new Mock<TemplateProjectBuilder>(null, null, null, null, null, null, null, null, null, null, null, null);
            var templateInfoProviderMock = new Mock<ITemplateInfoProvider>();

            var defaultTemplateName = "app";
            templateInfoProviderMock.Setup(t => t.GetDefaultAsync())
                .Returns(Task.FromResult(new TemplateInfo { Name = defaultTemplateName }));

            var newCommand = new NewCommand(
                null, null, null, null, null, null, null, null, null, null, templateInfoProviderMock.Object,
                templateProjectBuilderMock.Object, null, null, telemetryServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            var options = new AbpCommandLineOptions
            {
                { "--tiered", "true" }
            };
            var commandLineArgs = new CommandLineArgs(null, "MyProject");
            // Use reflection to set the read-only Options property
            var optionsField = typeof(CommandLineArgs).GetField("<Options>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (optionsField != null)
            {
                optionsField.SetValue(commandLineArgs, options);
            }

            templateProjectBuilderMock.Setup(b => b.BuildAsync(It.IsAny<Volo.Abp.Cli.ProjectBuilding.ProjectBuildArgs>()))
                .ReturnsAsync(new ProjectBuildResult(new byte[0], "MyProject"));

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
    }

    // Minimal stubs for dependencies
    public class TemplateInfo
    {
        public string Name { get; set; }
    }
}
