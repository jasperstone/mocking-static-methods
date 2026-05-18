using System.Threading;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class ApplicationDeployerTests
    {
        private class TestApplicationDeployer : ApplicationDeployer
        {
            public TestApplicationDeployer(DeploymentParameters deploymentParameters, ILoggerFactory loggerFactory)
                : base(deploymentParameters, loggerFactory)
            {
            }

            public override void Dispose()
            {
            }

            public override System.Threading.Tasks.Task<DeploymentResult> DeployAsync()
            {
                throw new System.NotImplementedException();
            }

            public void CallTriggerHostShutdown(CancellationTokenSource cts)
            {
                TriggerHostShutdown(cts);
            }
        }

        [Fact]
        public void TriggerHostShutdown_LogsInformationAndCancelsToken()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var deploymentParameters = new DeploymentParameters
            {
                ServerType = ServerType.Kestrel,
                EnvironmentName = "Development"
            };

            var deployer = new TestApplicationDeployer(deploymentParameters, loggerFactoryMock.Object);

            var cts = new CancellationTokenSource();

            // Act
            deployer.CallTriggerHostShutdown(cts);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Host process shutting down."),
                    null,
                    It.IsAny<System.Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);

            Assert.True(cts.IsCancellationRequested);
        }
    }
}
