using Xunit;
using Moq;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;

namespace Volo.Abp.Cli.Tests
{
    public class SuiteCommandTests
    {
        [Fact]
        public void StartSuite_PortAlreadyInUse_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommand(
                null, null, null, null, null, null);
            suiteCommand.Logger = loggerMock.Object;
            suiteCommand._abpSuitePort = 3000;

            // Act
            var process = suiteCommand.StartSuite();

            // Assert
            loggerMock.Verify(
                l => l.LogError(It.Is<string>(s => s.Contains("Port \"3000\" is already in use."))),
                Times.Once);
        }

        [Fact]
        public void IsPortAlreadyInUse_PortInUse_ReturnsTrue()
        {
            // Arrange
            var suiteCommand = new SuiteCommand(
                null, null, null, null, null, null);
            suiteCommand._abpSuitePort = 3000;

            // Act
            var result = suiteCommand.IsPortAlreadyInUse();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsPortAlreadyInUse_PortNotInUse_ReturnsFalse()
        {
            // Arrange
            var suiteCommand = new SuiteCommand(
                null, null, null, null, null, null);
            suiteCommand._abpSuitePort = 3001; // assuming this port is not in use

            // Act
            var result = suiteCommand.IsPortAlreadyInUse();

            // Assert
            Assert.False(result);
        }
    }
}
