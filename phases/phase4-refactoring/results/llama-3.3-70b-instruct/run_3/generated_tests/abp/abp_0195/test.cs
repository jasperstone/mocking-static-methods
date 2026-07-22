using Xunit;
using Moq;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Microsoft.Extensions.Logging;

namespace Volo.Abp.Cli.Tests
{
    public class ListModulesCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsModuleList()
        {
            // Arrange
            var moduleInfoProviderMock = new Mock<IModuleInfoProvider>();
            var loggerMock = new Mock<ILogger<ListModulesCommand>>();
            var telemetryServiceMock = new Mock<ITelemetryService>();
            var command = new ListModulesCommand(moduleInfoProviderMock.Object, telemetryServiceMock.Object);
            command.Logger = loggerMock.Object;

            var modules = new List<ModuleInfo>
            {
                new ModuleInfo { DisplayName = "Module 1", Name = "Module1", IsPro = false },
                new ModuleInfo { DisplayName = "Module 2", Name = "Module2", IsPro = false }
            };

            moduleInfoProviderMock.Setup(m => m.GetModuleListAsync()).ReturnsAsync(modules);

            // Act
            await command.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
        }
    }

    public class ModuleInfo
    {
        public string DisplayName { get; set; }
        public string Name { get; set; }
        public bool IsPro { get; set; }
    }
}
