using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Building;
using Volo.Abp.Internal.Telemetry;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests
{
    public class ListModulesCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_ShouldLogModuleList()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ListModulesCommand>>();
            var mockTelemetryService = new Mock<ITelemetryService>();

            var modules = new List<ModuleInfo>
            {
                new ModuleInfo { Name = "Module1", DisplayName = "Module One", IsPro = false },
                new ModuleInfo { Name = "Module2", DisplayName = "Module Two", IsPro = true }
            };

            var mockModuleInfoProvider = new Mock<ModuleInfoProvider>(MockBehavior.Strict);
            mockModuleInfoProvider.Setup(m => m.GetModuleListAsync()).ReturnsAsync(modules);

            var command = new ListModulesCommand(mockModuleInfoProvider.Object, mockTelemetryService.Object)
            {
                Logger = mockLogger.Object
            };

            var commandLineArgs = new CommandLineArgs();

            // Act
            await command.ExecuteAsync(commandLineArgs);

            // Assert
            mockLogger.Verify(
                x => x.LogInformation(
                    It.Is<string>(s => s.Contains("Open Source Application Modules") &&
                                        s.Contains("> Module One") &&
                                        s.Contains("> Module Two")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<string, Exception, string>>()),
                Times.Once);
        }
    }
}
