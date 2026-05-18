using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class ApplicationDeployerTests
    {
        [Fact]
        public void TriggerHostShutdown_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var deploymentParameters = new DeploymentParameters();
            var deployer = new Mock<MockApplicationDeployer>(deploymentParameters, Mock.Of<ILoggerFactory>())
            {
                CallBase = true
            };
            deployer.Setup(d => d.Logger).Returns(loggerMock.Object);

            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            deployer.Object.TriggerHostShutdown(cancellationTokenSource);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Host process shutting down.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }

    public class MockApplicationDeployer : ApplicationDeployer
    {
        public MockApplicationDeployer(DeploymentParameters deploymentParameters, ILoggerFactory loggerFactory)
            : base(deploymentParameters, loggerFactory)
        {
        }

        public override void Dispose()
        {
            // No-op
        }

        public override Task<DeploymentResult> DeployAsync()
        {
            return Task.FromResult(new DeploymentResult());
        }
    }
}
