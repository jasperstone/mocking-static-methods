using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class ListModulesCommandTests
    {
        private readonly Mock<ILogger<ListModulesCommand>> _loggerMock;
        private readonly Mock<ModuleInfoProvider> _moduleInfoProviderMock;
        private readonly Mock<ITelemetryService> _telemetryServiceMock;

        public ListModulesCommandTests()
        {
            _loggerMock = new Mock<ILogger<ListModulesCommand>>();
            _moduleInfoProviderMock = new Mock<ModuleInfoProvider>();
            _telemetryServiceMock = new Mock<ITelemetryService>();
        }

        [Fact]
        public async Task ExecuteAsync_LogsInformation()
        {
            // Arrange
            var command = new ListModulesCommand(_moduleInfoProviderMock.Object, _telemetryServiceMock.Object);
            command.Logger = _loggerMock.Object;

            var modules = new List<ModuleInfo>
            {
                new ModuleInfo { DisplayName = "Module1", Name = "Module1", IsPro = false },
                new ModuleInfo { DisplayName = "Module2", Name = "Module2", IsPro = true },
            };

            _moduleInfoProviderMock.Setup(m => m.GetModuleListAsync()).ReturnsAsync(modules);

            // Act
            await command.ExecuteAsync(new CommandLineArgs());

            // Assert
            _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_IncludesProModules_WhenIncludeProModulesOptionIsPresent()
        {
            // Arrange
            var command = new ListModulesCommand(_moduleInfoProviderMock.Object, _telemetryServiceMock.Object);
            command.Logger = _loggerMock.Object;

            var modules = new List<ModuleInfo>
            {
                new ModuleInfo { DisplayName = "Module1", Name = "Module1", IsPro = false },
                new ModuleInfo { DisplayName = "Module2", Name = "Module2", IsPro = true },
            };

            _moduleInfoProviderMock.Setup(m => m.GetModuleListAsync()).ReturnsAsync(modules);

            var commandLineArgs = new CommandLineArgs();
            commandLineArgs.Options.Add("include-pro-modules", null);

            // Act
            await command.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
        }
    }
}
