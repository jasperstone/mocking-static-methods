using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;

namespace ApplicationDeployerTests
{
    public class DummyDeployer : ApplicationDeployer
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

    public class ApplicationDeployerLoggingTests
    {
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly DeploymentParameters _deploymentParameters;

        public ApplicationDeployerLoggingTests()
        {
            _loggerMock = new Mock<ILogger>();
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(_loggerMock.Object);

            _deploymentParameters = new DeploymentParameters
            {
                EnvironmentName = "Development",
                UserAdditionalCleanup = null
            };
        }

        [Fact]
        public void TriggerHostShutdown_LogsInformationAndCancels()
        {
            // Arrange
            var deployer = new DummyDeployer(_deploymentParameters, _loggerFactoryMock.Object);
            var cts = new CancellationTokenSource();

            // Act
            deployer.TriggerHostShutdown(cts);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
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
