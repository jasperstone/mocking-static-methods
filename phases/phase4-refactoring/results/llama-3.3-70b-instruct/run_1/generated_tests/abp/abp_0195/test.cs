using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli;

namespace Volo.Abp.Cli.Tests
{
    public class ListModulesCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsInformation()
        {
            // Arrange
            var moduleInfoProviderMock = new Mock<Volo.Abp.Cli.Commands.ModuleInfoProvider>();
            var loggerMock = new Mock<ILogger<ListModulesCommand>>();
            var command = new ListModulesCommand(moduleInfoProviderMock.Object, null)
            {
                Logger = loggerMock.Object
            };

            var modules = new[]
            {
                new Volo.Abp.Cli.Commands.ModuleInfo { DisplayName = "Module1", Name = "Module1", IsPro = false },
                new Volo.Abp.Cli.Commands.ModuleInfo { DisplayName = "Module2", Name = "Module2", IsPro = true }
            };

            moduleInfoProviderMock.Setup(m => m.GetModuleListAsync()).ReturnsAsync(modules);

            // Act
            await command.ExecuteAsync(new Volo.Abp.Cli.Args.CommandLineArgs());

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
        }
    }
}
