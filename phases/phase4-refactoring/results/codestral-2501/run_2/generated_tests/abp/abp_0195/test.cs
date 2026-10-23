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

namespace Volo.Abp.Cli.Core.Tests
{
    public class ListModulesCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_ShouldLogInformationWithCorrectOutput()
        {
            // Arrange
            var moduleInfoProviderMock = new Mock<ModuleInfoProvider>();
            var telemetryServiceMock = new Mock<ITelemetryService>();
            var loggerMock = new Mock<ILogger<ListModulesCommand>>();

            var commandLineArgs = new CommandLineArgs(new List<string> { "list-modules" }, new Dictionary<string, string>());
            var command = new ListModulesCommand(moduleInfoProviderMock.Object, telemetryServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            var modules = new List<ModuleInfo>
            {
                new ModuleInfo { DisplayName = "Module1", Name = "module1", IsPro = false },
                new ModuleInfo { DisplayName = "Module2", Name = "module2", IsPro = true }
            };

            moduleInfoProviderMock.Setup(m => m.GetModuleListAsync()).ReturnsAsync(modules);

            // Act
            await command.ExecuteAsync(commandLineArgs);

            // Assert
            var expectedOutput = $"{Environment.NewLine}Open Source Application Modules{Environment.NewLine}{Environment.NewLine}> Module1                                 (module1){Environment.NewLine}";
            loggerMock.Verify(logger => logger.LogInformation(expectedOutput), Times.Once);
        }
    }
}
