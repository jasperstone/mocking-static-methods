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
using Volo.Abp.Internal.Telemetry;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class ListModulesCommandTests
    {
        private readonly Mock<ModuleInfoProvider> _moduleInfoProviderMock;
        private readonly Mock<ITelemetryService> _telemetryServiceMock;
        private readonly Mock<ILogger<ListModulesCommand>> _loggerMock;
        private readonly ListModulesCommand _command;

        public ListModulesCommandTests()
        {
            _moduleInfoProviderMock = new Mock<ModuleInfoProvider>();
            _telemetryServiceMock = new Mock<ITelemetryService>();
            _loggerMock = new Mock<ILogger<ListModulesCommand>>();
            _command = new ListModulesCommand(_moduleInfoProviderMock.Object, _telemetryServiceMock.Object)
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task ExecuteAsync_ShouldLogModuleList()
        {
            // Arrange
            var modules = new List<ModuleInfo>
            {
                new ModuleInfo { Name = "Module1", DisplayName = "Module One", IsPro = false },
                new ModuleInfo { Name = "Module2", DisplayName = "Module Two", IsPro = true }
            };

            _moduleInfoProviderMock.Setup(x => x.GetModuleListAsync()).ReturnsAsync(modules);

            var commandLineArgs = new CommandLineArgs();
            commandLineArgs.Options.Add("include-pro-modules", "true");

            var expectedOutput = new StringBuilder(Environment.NewLine);
            expectedOutput.AppendLine("Open Source Application Modules");
            expectedOutput.AppendLine();
            expectedOutput.AppendLine("> Module One                                      (Module1)");
            expectedOutput.AppendLine();
            expectedOutput.AppendLine("Commercial (Pro) Application Modules");
            expectedOutput.AppendLine();
            expectedOutput.AppendLine("> Module Two                                      (Module2)");

            // Act
            await _command.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<string>(s => s == expectedOutput.ToString()),
                    It.IsAny<object[]>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()),
                Times.Once);
        }
    }

    public class ModuleInfo
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool IsPro { get; set; }
    }
}
