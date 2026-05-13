using Moq;
using System.Net.NetworkInformation;
using Xunit;
using Volo.Abp.Cli.Commands;
using Microsoft.Extensions.Logging;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class SuiteCommandTests
    {
        [Fact]
        public void StartSuite_PortAlreadyInUse_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var cmdHelperMock = new Mock<ICmdHelper>();
            var suiteCommand = new SuiteCommand(
                null, null, cmdHelperMock.Object, null, null, null)
            {
                Logger = loggerMock.Object
            };

            // Mock the IsPortAlreadyInUse method to return true
            suiteCommand.IsPortAlreadyInUse = () => true;

            // Act
            suiteCommand.StartSuite();

            // Assert
            loggerMock.Verify(
                l => l.LogError(It.Is<string>(s => s.Contains("Port \"3000\" is already in use."))),
                Times.Once);
        }
    }
}
