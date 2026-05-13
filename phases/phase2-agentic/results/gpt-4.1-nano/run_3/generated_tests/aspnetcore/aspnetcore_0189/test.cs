using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class ApplicationDeployerTests
    {
        private class DummyDeployer : ApplicationDeployer
        {
            public DummyDeployer(DeploymentParameters parameters, ILoggerFactory loggerFactory)
                : base(parameters, loggerFactory)
            {
            }

            public override Task<DeploymentResult> DeployAsync()
            {
                throw new NotImplementedException();
            }

            public override void Dispose()
            {
            }
        }

        [Fact]
        public void TriggerHostShutdown_LogsInformationAndCancels()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var deploymentParameters = new DeploymentParameters();
            var deployer = new DummyDeployer(deploymentParameters, loggerFactoryMock.Object);

            var cts = new CancellationTokenSource();

            // Act
            deployer.TriggerHostShutdown(cts);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Host process shutting down.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.True(cts.IsCancellationRequested);
        }
    }
}
