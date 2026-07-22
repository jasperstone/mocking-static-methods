using Xunit;
using Moq;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;

namespace Volo.Abp.Cli.Core.Tests
{
    public class SuiteCommandTests
    {
        [Fact]
        public void StartSuite_PortAlreadyInUse_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommand(
                null, null, null, null, null, null
            );
            suiteCommand.Logger = loggerMock.Object;

            // Act
            var process = suiteCommand.StartSuite();

            // Assert
            loggerMock.Verify(
                l => l.LogError(It.Is<string>(s => s.Contains("Port \"3000\" is already in use."))),
                Times.Once
            );
        }
    }
}
