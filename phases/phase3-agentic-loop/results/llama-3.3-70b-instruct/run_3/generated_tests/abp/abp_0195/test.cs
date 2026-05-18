using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Core;
using Volo.Abp.Cli.Commands;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Cli;

namespace Volo.Abp.Cli.Core.Tests
{
    public class ListModulesCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsModuleList()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ListModulesCommand>>();
            var moduleInfoProviderMock = new Mock<ModuleInfoProvider>();
            var telemetryServiceMock = new Mock<ITelemetryService>();
            var command = new ListModulesCommand(moduleInfoProviderMock.Object, telemetryServiceMock.Object);
            command.Logger = loggerMock.Object;

            var modules = new[]
            {
                new ModuleInfo { DisplayName = "Module 1", Name = "Module1", IsPro = false },
                new ModuleInfo { DisplayName = "Module 2", Name = "Module2", IsPro = false }
            };

            moduleInfoProviderMock.Setup(p => p.GetModuleListAsync()).ReturnsAsync(modules);

            // Act
            await command.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LogsModuleList_WithProModules()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ListModulesCommand>>();
            var moduleInfoProviderMock = new Mock<ModuleInfoProvider>();
            var telemetryServiceMock = new Mock<ITelemetryService>();
            var command = new ListModulesCommand(moduleInfoProviderMock.Object, telemetryServiceMock.Object);
            command.Logger = loggerMock.Object;

            var modules = new[]
            {
                new ModuleInfo { DisplayName = "Module 1", Name = "Module1", IsPro = false },
                new ModuleInfo { DisplayName = "Module 2", Name = "Module2", IsPro = true }
            };

            moduleInfoProviderMock.Setup(p => p.GetModuleListAsync()).ReturnsAsync(modules);

            var commandLineArgs = new CommandLineArgs();
            commandLineArgs.Options.Add("include-pro-modules", null);

            // Act
            await command.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
        }
    }
}
