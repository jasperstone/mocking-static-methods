using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Server.IntegrationTesting;
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
                // No resources to dispose in test
            }

            public void CallTriggerHostShutdown(CancellationTokenSource cts)
            {
                TriggerHostShutdown(cts);
            }

            public void CallStartTimer()
            {
                StartTimer();
            }

            public void CallStopTimer()
            {
                StopTimer();
            }
        }

        private DeploymentParameters CreateDeploymentParameters()
        {
            return new DeploymentParameters
            {
                ServerType = ServerType.Kestrel,
                EnvironmentName = "Development",
                ApplicationPath = "somepath",
                ApplicationName = "app",
                RuntimeFlavor = RuntimeFlavor.CoreClr,
                RuntimeArchitecture = RuntimeArchitecture.x64,
                TargetFramework = "net6.0"
            };
        }

        [Fact]
        public void TriggerHostShutdown_LogsInformationAndCancelsToken()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var deployer = new TestApplicationDeployer(CreateDeploymentParameters(), loggerFactoryMock.Object);
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
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.True(cts.IsCancellationRequested);
        }

        [Fact]
        public void StartTimer_LogsDeployingMessage()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var deployer = new TestApplicationDeployer(CreateDeploymentParameters(), loggerFactoryMock.Object);

            // Act
            deployer.CallStartTimer();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deploying")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void StopTimer_LogsElapsedTime()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var deployer = new TestApplicationDeployer(CreateDeploymentParameters(), loggerFactoryMock.Object);

            // Start timer to have elapsed time
            deployer.CallStartTimer();
            Thread.Sleep(10); // small delay to ensure some elapsed time

            // Act
            deployer.CallStopTimer();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().StartsWith("[Time]: Total time taken for this test variation")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
