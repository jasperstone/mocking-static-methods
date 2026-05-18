using System;
using System.Diagnostics;
using System.Threading;
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
            var deployer = new Mock<ApplicationDeployer>(Mock.Of<DeploymentParameters>(), Mock.Of<ILoggerFactory>())
            {
                CallBase = true
            };
            deployer.Setup(d => d.Logger).Returns(loggerMock.Object);

            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            deployer.Object.TriggerHostShutdown(cancellationTokenSource);

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Host process shutting down.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void StartTimer_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var deployer = new Mock<ApplicationDeployer>(Mock.Of<DeploymentParameters>(), Mock.Of<ILoggerFactory>())
            {
                CallBase = true
            };
            deployer.Setup(d => d.Logger).Returns(loggerMock.Object);

            // Act
            deployer.Object.StartTimer();

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deploying")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void StopTimer_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var deployer = new Mock<ApplicationDeployer>(Mock.Of<DeploymentParameters>(), Mock.Of<ILoggerFactory>())
            {
                CallBase = true
            };
            deployer.Setup(d => d.Logger).Returns(loggerMock.Object);

            // Act
            deployer.Object.StopTimer();

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("[Time]: Total time taken for this test variation")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
