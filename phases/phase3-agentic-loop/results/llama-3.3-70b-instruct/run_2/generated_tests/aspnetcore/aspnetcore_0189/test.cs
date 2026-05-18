using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class ApplicationDeployerTests : ApplicationDeployer
    {
        public ApplicationDeployerTests() : base(new DeploymentParameters(), new LoggerFactory()) { }

        public override void Dispose()
        {
            // No-op
        }

        [Fact]
        public void TriggerHostShutdown_LogsInformationAndCancelsTokenSource()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var cancellationTokenSource = new CancellationTokenSource();
            var deploymentParameters = new DeploymentParameters
            {
                ApplicationName = "TestApplication",
                ApplicationPath = "/path/to/application",
                EnvironmentName = "TestEnvironment",
                TargetFramework = "net6.0",
                RuntimeFlavor = RuntimeFlavor.CoreClr,
                RuntimeArchitecture = RuntimeArchitecture.X64,
                ServerType = ServerType.Kestrel,
                PublishedApplicationRootPath = "/path/to/published/application",
                PreservePublishedApplicationForDebugging = false,
            };
            var applicationDeployer = new TestApplicationDeployer(deploymentParameters, loggerMock.Object);

            // Act
            applicationDeployer.TriggerHostShutdown(cancellationTokenSource);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Host process shutting down."), Times.Once);
            Assert.True(cancellationTokenSource.IsCancellationRequested);
        }

        private class TestApplicationDeployer : ApplicationDeployer
        {
            public TestApplicationDeployer(DeploymentParameters deploymentParameters, ILogger logger) : base(deploymentParameters, new LoggerFactory())
            {
                Logger = logger;
            }
        }
    }
}
