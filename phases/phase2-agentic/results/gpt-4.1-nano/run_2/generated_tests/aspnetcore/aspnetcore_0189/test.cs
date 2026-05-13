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
        private class TestDeployer : ApplicationDeployer
        {
            public bool TriggerHostShutdownCalled { get; private set; }
            public bool DisposeCalled { get; private set; }

            public TestDeployer(DeploymentParameters parameters, ILoggerFactory loggerFactory)
                : base(parameters, loggerFactory)
            {
            }

            public override Task<DeploymentResult> DeployAsync()
            {
                throw new NotImplementedException();
            }

            protected override void Dispose()
            {
                DisposeCalled = true;
            }

            public void CallTriggerHostShutdown(CancellationTokenSource cts)
            {
                TriggerHostShutdown(cts);
                TriggerHostShutdownCalled = true;
            }
        }

        [Fact]
        public void TriggerHostShutdown_LogsInformationAndCancels()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var parameters = new DeploymentParameters
            {
                ServerType = ServerType.IIS,
                EnvironmentName = "Development"
            };

            var deployer = new TestDeployer(parameters, loggerFactoryMock.Object);
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
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
            Assert.True(cts.IsCancellationRequested);
        }
    }
}
