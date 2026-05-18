using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Threading;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting
{
    public class ApplicationDeployerTests
    {
        [Fact]
        public void TriggerHostShutdown_LogsInformationMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var deploymentParameters = new Mock<DeploymentParameters>().Object;
            var deployer = new TestApplicationDeployer(deploymentParameters, loggerFactoryMock.Object);

            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            deployer.TriggerHostShutdown(cancellationTokenSource);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v?.ToString() == "Host process shutting down."),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private class TestApplicationDeployer : ApplicationDeployer
        {
            public TestApplicationDeployer(DeploymentParameters deploymentParameters, ILoggerFactory loggerFactory)
                : base(deploymentParameters, loggerFactory)
            {
            }

            public override Task<DeploymentResult> DeployAsync() => Task.FromResult(new DeploymentResult());
            public override void Dispose() { }
        }
    }
}
