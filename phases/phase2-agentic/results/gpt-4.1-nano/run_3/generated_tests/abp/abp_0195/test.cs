using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;

namespace Volo.Abp.Cli.Tests
{
    public class ListModulesCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_Should_Log_Module_List_With_IncludeProModules()
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

            var args = new CommandLineArgs
            {
                Options = new Dictionary<string, string> { { "include-pro-modules", "" } }
            };

            // Act
            await command.ExecuteAsync(args);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(It.Is<string>(s => s.Contains("Module A") && s.Contains("Module B"))),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_Should_Log_Module_List_Without_ProModules_When_Option_Not_Present()
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

            var args = new CommandLineArgs
            {
                Options = new Dictionary<string, string>() // no include-pro-modules key
            };

            // Act
            await command.ExecuteAsync(args);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(It.Is<string>(s => s.Contains("Module A") && !s.Contains("Module B"))),
                Times.Once);
        }
    }

    // Mocked or simplified classes for compilation
    public class ModuleInfo
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool IsPro { get; set; }
    }
}
