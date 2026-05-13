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
using Volo.Abp.DependencyInjection;
using Volo.Abp.Internal.Telemetry;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class ListModulesCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_WhenCalled_LogsInformation()
        {
            // Arrange
            var moduleInfoProviderMock = new Mock<ModuleInfoProvider>();
            moduleInfoProviderMock
                .Setup(m => m.GetModuleListAsync())
                .ReturnsAsync(new List<ModuleInfo>
                {
                    new ModuleInfo { DisplayName = "Module1", Name = "Module1", IsPro = false },
                    new ModuleInfo { DisplayName = "Module2", Name = "Module2", IsPro = true }
                });

            var telemetryServiceMock = new Mock<ITelemetryService>();
            var loggerMock = new Mock<ILogger<ListModulesCommand>>();

            var command = new ListModulesCommand(moduleInfoProviderMock.Object, telemetryServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            var commandLineArgs = new CommandLineArgs
            {
                Options = new Dictionary<string, string>
                {
                    { "include-pro-modules", string.Empty }
                }
            };

            // Act
            await command.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(It.Is<string>(s => s.Contains("Open Source Application Modules") && s.Contains("Module1"))),
                Times.Once);

            loggerMock.Verify(
                l => l.LogInformation(It.Is<string>(s => s.Contains("Commercial (Pro) Application Modules") && s.Contains("Module2"))),
                Times.Once);
        }
    }

    public class ModuleInfo
    {
        public string DisplayName { get; set; }
        public string Name { get; set; }
        public bool IsPro { get; set; }
    }

    public class ModuleInfoProvider
    {
        public Task<List<ModuleInfo>> GetModuleListAsync()
        {
            throw new NotImplementedException();
        }
    }
}
