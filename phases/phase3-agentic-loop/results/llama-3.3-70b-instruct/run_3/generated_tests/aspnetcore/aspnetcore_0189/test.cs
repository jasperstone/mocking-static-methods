using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
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
            var loggerFactory = new LoggerFactory();
            var applicationDeployer = new TestApplicationDeployer(deploymentParameters, loggerFactory);
            var hostShutdownSource = new CancellationTokenSource();

            // Act
            applicationDeployer.TriggerHostShutdown(hostShutdownSource);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Host process shutting down."), Times.Once);
        }

        private class TestApplicationDeployer : ApplicationDeployer
        {
            private readonly ILogger _logger;

            public TestApplicationDeployer(DeploymentParameters deploymentParameters, ILoggerFactory loggerFactory) 
                : base(deploymentParameters, loggerFactory)
            {
                _logger = loggerFactory.CreateLogger(GetType().FullName);
            }

            public override Task<DeploymentResult> DeployAsync()
            {
                throw new NotImplementedException();
            }

            public override void Dispose()
            {
                throw new NotImplementedException();
            }

            protected override void TriggerHostShutdown(CancellationTokenSource hostShutdownSource)
            {
                _logger.LogInformation("Host process shutting down.");
                try
                {
                    hostShutdownSource.Cancel();
                }
                catch (Exception)
                {
                    // Suppress errors.
                }
            }
        }
    }
}
