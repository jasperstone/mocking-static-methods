using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
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
        public void KillSuite_LogsInformation_WhenSuiteIsClosed()
        {
            // Arrange
            var processMock = new Mock<Process>();
            _cmdHelperMock.Setup(c => c.RunCmdAndGetProcess(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(processMock.Object);

            // Act
            _suiteCommand.KillSuite();

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Suite closed."),
                Times.Once);
        }

        [Fact]
        public void KillSuite_LogsError_WhenExceptionIsThrown()
        {
            // Arrange
            var processMock = new Mock<Process>();
            _cmdHelperMock.Setup(c => c.RunCmdAndGetProcess(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception("Test exception"));

            // Act
            _suiteCommand.KillSuite();

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Cannot close Suite.Test exception"),
                Times.Once);
        }
    }
}
