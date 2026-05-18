using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting
{
    public class ApplicationDeployerTests
    {
        [Fact]
        public void TriggerHostShutdown_LogsInformationMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationDeployer>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            
            // Create mock deployer - use protected constructor via mock
            var deployerMock = new Mock<ApplicationDeployer>(It.IsAny<DeploymentParameters>(), loggerFactoryMock.Object) 
            { 
                CallBase = true 
            }.Object;
            
            var cts = new CancellationTokenSource();

            // Act
            deployerMock.TriggerHostShutdown(cts);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Host process shutting down.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
