using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.ModuleInfo;
using Volo.Abp.Internal.Telemetry;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class ListModulesCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsModuleList()
        {
            // Arrange
            var moduleInfoProviderMock = new Mock<IModuleInfoProvider>();
            var moduleInfo1 = new ModuleInfo { DisplayName = "Module 1", Name = "Module1", IsPro = false };
            var moduleInfo2 = new ModuleInfo { DisplayName = "Module 2", Name = "Module2", IsPro = true };
            moduleInfoProviderMock.Setup(p => p.GetModuleListAsync()).ReturnsAsync(new List<ModuleInfo> { moduleInfo1, moduleInfo2 });
            var loggerMock = new Mock<ILogger<ListModulesCommand>>();
            var command = new ListModulesCommand(moduleInfoProviderMock.Object, new Mock<ITelemetryService>().Object);
            command.Logger = loggerMock.Object;

            // Act
            await command.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
        }
    }
}
