using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class SuiteCommandTests
    {
        private readonly SuiteCommand _suiteCommand;
        private readonly Mock<ILogger<SuiteCommand>> _loggerMock;
        private readonly Mock<ICmdHelper> _cmdHelperMock;

        public SuiteCommandTests()
        {
            _loggerMock = new Mock<ILogger<SuiteCommand>>();
            _cmdHelperMock = new Mock<ICmdHelper>();

            _suiteCommand = new SuiteCommand(
                null,
                null,
                _cmdHelperMock.Object,
                null,
                null,
                null)
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public void KillSuite_ShouldLogInformation_WhenSuiteProcessesAreKilled()
        {
            // Arrange
            var suiteProcesses = new List<Process>
            {
                new Process { ProcessName = "abp-suite" },
                new Process { ProcessName = "abp-suite" }
            };

            _cmdHelperMock.Setup(c => c.GetProcessesRelatedWithSuite()).Returns(suiteProcesses);

            // Act
            _suiteCommand.KillSuite();

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Suite closed."),
                Times.Exactly(suiteProcesses.Count));
        }

        [Fact]
        public void KillSuite_ShouldLogError_WhenExceptionIsThrown()
        {
            // Arrange
            var suiteProcesses = new List<Process>
            {
                new Process { ProcessName = "abp-suite" },
                new Process { ProcessName = "abp-suite" }
            };

            _cmdHelperMock.Setup(c => c.GetProcessesRelatedWithSuite()).Returns(suiteProcesses);

            foreach (var process in suiteProcesses)
            {
                process.Start();
            }

            // Act
            _suiteCommand.KillSuite();

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(It.IsAny<string>()),
                Times.Never);

            _loggerMock.Verify(
                x => x.LogInformation("Cannot close Suite." + It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public void IsPortAlreadyInUse_ShouldReturnTrue_WhenPortIsInUse()
        {
            // Arrange
            var ipGP = IPGlobalProperties.GetIPGlobalProperties();
            var endpoints = ipGP.GetActiveTcpListeners();
            var portInUse = endpoints.FirstOrDefault(e => e.Port == 3000);

            // Act
            var result = _suiteCommand.IsPortAlreadyInUse();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsPortAlreadyInUse_ShouldReturnFalse_WhenPortIsNotInUse()
        {
            // Arrange
            var ipGP = IPGlobalProperties.GetIPGlobalProperties();
            var endpoints = ipGP.GetActiveTcpListeners();
            var portInUse = endpoints.FirstOrDefault(e => e.Port == 3001);

            // Act
            var result = _suiteCommand.IsPortAlreadyInUse();

            // Assert
            Assert.False(result);
        }
    }
}
