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
            var hostShutdownSource = new CancellationTokenSource();
            var applicationDeployer = new TestApplicationDeployer(loggerMock.Object);

            // Act
            applicationDeployer.TriggerHostShutdown(hostShutdownSource);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Host process shutting down."), Times.Once);
        }

        private class TestApplicationDeployer : ApplicationDeployer
        {
            public TestApplicationDeployer(ILogger logger) 
                : base(new DeploymentParameters(), new LoggerFactory().CreateLogger(GetType().FullName))
            {
                Logger = logger;
            }

            public override Task<DeploymentResult> DeployAsync()
            {
                throw new NotImplementedException();
            }

            public override void Dispose()
            {
                throw new NotImplementedException();
            }
        }
    }
}
