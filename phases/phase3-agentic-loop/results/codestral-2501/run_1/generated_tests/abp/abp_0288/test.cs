using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class SuiteCommandTests
    {
        private readonly Mock<ILogger<SuiteCommand>> _loggerMock;
        private readonly Mock<ICmdHelper> _cmdHelperMock;
        private readonly Mock<SuiteAppSettingsService> _suiteAppSettingsServiceMock;
        private readonly SuiteCommand _suiteCommand;

        public SuiteCommandTests()
        {
            _loggerMock = new Mock<ILogger<SuiteCommand>>();
            _cmdHelperMock = new Mock<ICmdHelper>();
            _suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>(_cmdHelperMock.Object);

            _suiteCommand = new SuiteCommand(
                null,
                null,
                _cmdHelperMock.Object,
                null,
                null,
                _suiteAppSettingsServiceMock.Object)
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async void ExecuteAsync_WhenExceptionThrownInKillSuite_LogsErrorMessage()
        {
            // Arrange
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.Kill()).Throws(new Exception("Test exception"));

            var processes = new List<Process> { processMock.Object };

            _cmdHelperMock.Setup(c => c.GetProcessesRelatedWithSuite()).Returns(processes);

            var commandLineArgs = new CommandLineArgs
            {
                Target = "generate",
                Options = new Dictionary<string, string>
                {
                    { "entity", "entity.json" },
                    { "solution", "solution.sln" }
                }
            };

            // Act
            await _suiteCommand.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogInformation(
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Cannot close Suite.Test exception")),
                    It.IsAny<Exception>()),
                Times.Once);
        }
    }
}
