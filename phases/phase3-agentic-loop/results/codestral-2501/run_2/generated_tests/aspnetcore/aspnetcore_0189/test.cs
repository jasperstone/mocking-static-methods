using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ApplicationDeployerTests
{
    public class ApplicationDeployerTests
    {
        [Fact]
        public void TriggerHostShutdown_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var deploymentParameters = new DeploymentParameters();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var deployer = new Mock<ApplicationDeployer>(deploymentParameters, loggerFactoryMock.Object) { CallBase = true };
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

    // Mock ApplicationDeployer class for testing purposes
    public class ApplicationDeployer : IDisposable
    {
        protected ILogger Logger { get; }

        public ApplicationDeployer(DeploymentParameters deploymentParameters, ILoggerFactory loggerFactory)
        {
            Logger = loggerFactory.CreateLogger(GetType().FullName);
        }

        protected void TriggerHostShutdown(CancellationTokenSource hostShutdownSource)
        {
            Logger.LogInformation("Host process shutting down.");
            try
            {
                hostShutdownSource.Cancel();
            }
            catch (Exception)
            {
                // Suppress errors.
            }
        }

        public void Dispose()
        {
            // Dispose logic
        }
    }
}
