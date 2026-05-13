using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
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
            var suiteProcesses = new List<Process>
            {
                new Process { ProcessName = "abp-suite" }
            };

            _cmdHelperMock.Setup(c => c.RunCmdAndGetProcess(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(suiteProcesses.First());

            // Act
            _suiteCommand.KillSuite();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Suite closed.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [Fact]
        public void KillSuite_LogsError_WhenExceptionIsThrown()
        {
            // Arrange
            var suiteProcesses = new List<Process>
            {
                new Process { ProcessName = "abp-suite" }
            };

            _cmdHelperMock.Setup(c => c.RunCmdAndGetProcess(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(suiteProcesses.First());

            suiteProcesses.First().Kill();

            // Act
            _suiteCommand.KillSuite();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Cannot close Suite.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}
