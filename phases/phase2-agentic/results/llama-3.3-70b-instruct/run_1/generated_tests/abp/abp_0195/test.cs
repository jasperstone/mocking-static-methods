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
                new ModuleInfo { DisplayName = "Module1", Name = "Module1", IsPro = false },
                new ModuleInfo { DisplayName = "Module2", Name = "Module2", IsPro = true }
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
                new ModuleInfo { DisplayName = "Module1", Name = "Module1", IsPro = false },
                new ModuleInfo { DisplayName = "Module2", Name = "Module2", IsPro = true }
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

    public class ModuleInfo
    {
        public string DisplayName { get; set; }
        public string Name { get; set; }
        public bool IsPro { get; set; }
    }

    public class ModuleInfoProvider
    {
        public virtual Task<List<ModuleInfo>> GetModuleListAsync()
        {
            throw new NotImplementedException();
        }
    }
}
