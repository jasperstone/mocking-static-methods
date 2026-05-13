using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;
using Moq;

namespace Volo.Abp.Cli.Tests
{
    public class ListModulesCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_Should_LogCorrectOutput_When_IncludeProModulesOptionIsPresent()
        {
            // Arrange
            var moduleList = new List<ModuleInfo>
            {
                new ModuleInfo { Name = "ModuleA", DisplayName = "Module A", IsPro = false },
                new ModuleInfo { Name = "ModuleB", DisplayName = "Module B", IsPro = true }
            };

            var moduleInfoProviderMock = new Mock<ModuleInfoProvider>();
            moduleInfoProviderMock
                .Setup(m => m.GetModuleListAsync())
                .ReturnsAsync(moduleList);

            var telemetryServiceMock = new Mock<ITelemetryService>();
            telemetryServiceMock
                .Setup(t => t.TrackActivityAsync(It.IsAny<string>()))
                .ReturnsAsync(Mock.Of<IDisposable>());

            var loggerMock = new Mock<ILogger<ListModulesCommand>>();

            var command = new ListModulesCommand(moduleInfoProviderMock.Object, telemetryServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            var options = new Dictionary<string, string>
            {
                { "include-pro-modules", "true" }
            };
            var commandLineArgs = new CommandLineArgs(options);

            // Act
            await command.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(It.Is<string>(s => s.Contains("Open Source Application Modules") && s.Contains("Commercial (Pro) Application Modules"))),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_Should_LogCorrectOutput_When_IncludeProModulesOptionIsAbsent()
        {
            // Arrange
            var moduleList = new List<ModuleInfo>
            {
                new ModuleInfo { Name = "ModuleA", DisplayName = "Module A", IsPro = false },
                new ModuleInfo { Name = "ModuleB", DisplayName = "Module B", IsPro = true }
            };

            var moduleInfoProviderMock = new Mock<ModuleInfoProvider>();
            moduleInfoProviderMock
                .Setup(m => m.GetModuleListAsync())
                .ReturnsAsync(moduleList);

            var telemetryServiceMock = new Mock<ITelemetryService>();
            telemetryServiceMock
                .Setup(t => t.TrackActivityAsync(It.IsAny<string>()))
                .ReturnsAsync(Mock.Of<IDisposable>());

            var loggerMock = new Mock<ILogger<ListModulesCommand>>();

            var command = new ListModulesCommand(moduleInfoProviderMock.Object, telemetryServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            var options = new Dictionary<string, string>();
            var commandLineArgs = new CommandLineArgs(options);

            // Act
            await command.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(It.Is<string>(s => s.Contains("Open Source Application Modules") && !s.Contains("Commercial (Pro) Application Modules"))),
                Times.Once);
        }
    }

    // Mock classes for dependencies
    public class ModuleInfo
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool IsPro { get; set; }
    }

    public class CommandLineArgs
    {
        public Dictionary<string, string> Options { get; }

        public CommandLineArgs(Dictionary<string, string> options)
        {
            Options = options;
        }
    }
}
