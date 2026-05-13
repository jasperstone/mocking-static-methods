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
        public async Task ExecuteAsync_LogsInformationIncludingTieredYes_WhenTieredOptionIsPresent()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var telemetryServiceMock = new Mock<ITelemetryService>();
            var templateProjectBuilderMock = new Mock<TemplateProjectBuilder>(MockBehavior.Strict, 
                null, null, null, null, null, null, null, null, null, null, null, null);
            var templateInfoProviderMock = new Mock<ITemplateInfoProvider>();
            var defaultTemplateName = "app";
            templateInfoProviderMock.Setup(t => t.GetDefaultAsync())
                .ReturnsAsync(new TemplateInfo { Name = defaultTemplateName });

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
            var loggerField = typeof(NewCommand).BaseType.GetProperty("Logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(newCommand, loggerMock.Object);

            var options = new Dictionary<string, string>
            {
                { "tiered", "true" }
            };
            var commandLineArgs = new CommandLineArgs("new", "MyProject")
            {
                Options = new AbpCommandLineOptions(options)
            };

            // Setup TemplateProjectBuilder.BuildAsync to return dummy result
            var dummyProjectBuildArgs = new ProjectBuildArgs { OutputFolder = "output" };
            var dummyBuildResult = new object();
            templateProjectBuilderMock.Setup(t => t.BuildAsync(It.IsAny<ProjectBuildArgs>()))
                .ReturnsAsync(dummyBuildResult);

            // Setup GetProjectBuildArgsAsync to return dummyProjectBuildArgs
            var getProjectBuildArgsAsyncMethod = typeof(NewCommand).GetMethod("GetProjectBuildArgsAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            getProjectBuildArgsAsyncMethod.Invoke(newCommand, new object[] { commandLineArgs, defaultTemplateName, "MyProject" });

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
        }
    }

    // Minimal stubs for dependencies
    public class TemplateInfo
    {
        public string Name { get; set; }
    }

    public class ProjectBuildArgs
    {
        public string OutputFolder { get; set; }
        public Dictionary<string, object> ExtraProperties { get; } = new Dictionary<string, object>();
    }

    public class AbpCommandLineOptions : Dictionary<string, string>
    {
        public AbpCommandLineOptions() : base(StringComparer.OrdinalIgnoreCase) { }
        public AbpCommandLineOptions(IDictionary<string, string> dictionary) : base(dictionary, StringComparer.OrdinalIgnoreCase) { }

        public string GetOrNull(string shortKey, string longKey)
        {
            if (TryGetValue(shortKey, out var value)) return value;
            if (TryGetValue(longKey, out value)) return value;
            return null;
        }
    }
}
