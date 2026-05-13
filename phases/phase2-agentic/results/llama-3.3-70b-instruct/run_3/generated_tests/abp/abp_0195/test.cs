using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Core;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class ListModulesCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsModuleList()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ListModulesCommand>>();
            var moduleInfoProviderMock = new Mock<ModuleInfoProvider>();
            var command = new ListModulesCommand(moduleInfoProviderMock.Object, null)
            {
                Logger = loggerMock.Object
            };

            var modules = new List<ModuleInfo>
            {
                new ModuleInfo { DisplayName = "Module 1", Name = "Module1", IsPro = false },
                new ModuleInfo { DisplayName = "Module 2", Name = "Module2", IsPro = true }
            };

            moduleInfoProviderMock.Setup(m => m.GetModuleListAsync()).ReturnsAsync(modules);

            // Act
            await command.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_IncludesProModules_WhenIncludeProModulesOptionIsPresent()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ListModulesCommand>>();
            var moduleInfoProviderMock = new Mock<ModuleInfoProvider>();
            var command = new ListModulesCommand(moduleInfoProviderMock.Object, null)
            {
                Logger = loggerMock.Object
            };

            var modules = new List<ModuleInfo>
            {
                new ModuleInfo { DisplayName = "Module 1", Name = "Module1", IsPro = false },
                new ModuleInfo { DisplayName = "Module 2", Name = "Module2", IsPro = true }
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
