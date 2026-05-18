using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class ListModulesCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsModuleList()
        {
            // Arrange
            var moduleInfoProviderMock = new Mock<Volo.Abp.Cli.ModuleInfo.ModuleInfoProvider>();
            var telemetryServiceMock = new Mock<ITelemetryService>();
            var loggerMock = new Mock<ILogger<ListModulesCommand>>();

            var command = new ListModulesCommand(moduleInfoProviderMock.Object, telemetryServiceMock.Object);
            command.Logger = loggerMock.Object;

            var modules = new List<Volo.Abp.Cli.ModuleInfo.ModuleInfo>
            {
                new Volo.Abp.Cli.ModuleInfo.ModuleInfo { DisplayName = "Module 1", Name = "Module1", IsPro = false },
                new Volo.Abp.Cli.ModuleInfo.ModuleInfo { DisplayName = "Module 2", Name = "Module2", IsPro = true },
            };

            moduleInfoProviderMock.Setup(m => m.GetModuleListAsync()).ReturnsAsync(modules);

            // Act
            await command.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_IncludesProModules_WhenOptionIsPresent()
        {
            // Arrange
            var moduleInfoProviderMock = new Mock<Volo.Abp.Cli.ModuleInfo.ModuleInfoProvider>();
            var telemetryServiceMock = new Mock<ITelemetryService>();
            var loggerMock = new Mock<ILogger<ListModulesCommand>>();

            var command = new ListModulesCommand(moduleInfoProviderMock.Object, telemetryServiceMock.Object);
            command.Logger = loggerMock.Object;

            var modules = new List<Volo.Abp.Cli.ModuleInfo.ModuleInfo>
            {
                new Volo.Abp.Cli.ModuleInfo.ModuleInfo { DisplayName = "Module 1", Name = "Module1", IsPro = false },
                new Volo.Abp.Cli.ModuleInfo.ModuleInfo { DisplayName = "Module 2", Name = "Module2", IsPro = true },
            };

            moduleInfoProviderMock.Setup(m => m.GetModuleListAsync()).ReturnsAsync(modules);

            var commandLineArgs = new CommandLineArgs();
            commandLineArgs.Options.Add("include-pro-modules", null);

            // Act
            await command.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
        }
    }
}
