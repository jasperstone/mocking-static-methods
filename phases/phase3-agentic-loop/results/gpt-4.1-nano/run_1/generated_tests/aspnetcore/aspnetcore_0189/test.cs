using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;

namespace ApplicationDeployerTests
{
    public class ApplicationDeployerLoggingTests
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

            public void CallTriggerHostShutdown(CancellationTokenSource cts)
            {
                TriggerHostShutdown(cts);
            }

            public void CallInvokeUserApplicationCleanup()
            {
                InvokeUserApplicationCleanup();
            }

            public void CallAddEnvironmentVariablesToProcess(ProcessStartInfo startInfo, IDictionary<string, string> envVars)
            {
                AddEnvironmentVariablesToProcess(startInfo, envVars);
            }
        }

        private class DummyDeploymentParameters : DeploymentParameters
        {
            public override string EnvironmentName { get; set; } = "Development";
            public override Action<DeploymentParameters> UserAdditionalCleanup { get; set; }
        }

        [Fact]
        public void TriggerHostShutdown_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var parameters = new DummyDeploymentParameters();
            var deployer = new DummyDeployer(parameters, loggerFactoryMock.Object);

            var cts = new CancellationTokenSource();

            // Act
            deployer.CallTriggerHostShutdown(cts);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Host process shutting down.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
