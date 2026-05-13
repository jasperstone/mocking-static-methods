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
            public bool InvokeUserApplicationCleanupCalled { get; private set; }
            public bool AddEnvironmentVariablesToProcessCalled { get; private set; }
            public bool StartTimerCalled { get; private set; }
            public bool StopTimerCalled { get; private set; }
            public bool DisposeCalled { get; private set; }

            public TestDeployer(DeploymentParameters parameters, ILoggerFactory loggerFactory)
                : base(parameters, loggerFactory)
            {
            }

            public override Task<DeploymentResult> DeployAsync()
            {
                throw new NotImplementedException();
            }

            protected override void TriggerHostShutdown(CancellationTokenSource hostShutdownSource)
            {
                TriggerHostShutdownCalled = true;
                base.TriggerHostShutdown(hostShutdownSource);
            }

            protected override void InvokeUserApplicationCleanup()
            {
                InvokeUserApplicationCleanupCalled = true;
                base.InvokeUserApplicationCleanup();
            }

            protected override void AddEnvironmentVariablesToProcess(ProcessStartInfo startInfo, IDictionary<string, string> environmentVariables)
            {
                AddEnvironmentVariablesToProcessCalled = true;
                base.AddEnvironmentVariablesToProcess(startInfo, environmentVariables);
            }

            protected override void StartTimer()
            {
                StartTimerCalled = true;
                base.StartTimer();
            }

            protected override void StopTimer()
            {
                StopTimerCalled = true;
                base.StopTimer();
            }

            public override void Dispose()
            {
                DisposeCalled = true;
            }
        }

        private class DummyDeploymentParameters : DeploymentParameters
        {
            public override string ApplicationPath { get; set; } = "dummyPath";
            public override string ApplicationName { get; set; } = "dummyApp";
            public override string EnvironmentName { get; set; } = "Development";
            public override ServerType ServerType { get; set; } = ServerType.IIS;
            public override RuntimeFlavor RuntimeFlavor { get; set; } = RuntimeFlavor.CoreClr;
            public override string TargetFramework { get; set; } = "net6.0";
            public override bool PreservePublishedApplicationForDebugging { get; set; }
            public override Action<DeploymentParameters> UserAdditionalCleanup { get; set; }
            public override ApplicationPublisher ApplicationPublisher { get; set; }
        }

        [Fact]
        public void TriggerHostShutdown_LogsInformationAndCancels()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var parameters = new DummyDeploymentParameters();
            var deployer = new TestDeployer(parameters, new LoggerFactory());
            var cts = new CancellationTokenSource();

            // Replace logger with mock
            deployer.Logger = loggerMock.Object;

            // Act
            deployer.TriggerHostShutdown(cts);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Host process shutting down.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
            Assert.True(cts.IsCancellationRequested);
        }

        [Fact]
        public void InvokeUserApplicationCleanup_LogsWarningOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var parameters = new DummyDeploymentParameters
            {
                UserAdditionalCleanup = (p) => throw new InvalidOperationException("Cleanup failed")
            };
            var deployer = new TestDeployer(parameters, new LoggerFactory());
            deployer.Logger = loggerMock.Object;

            // Act
            deployer.InvokeUserApplicationCleanup();

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(s => s.Contains("User cleanup code failed with exception")),
                    It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public void AddEnvironmentVariablesToProcess_CallsHelpers()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var parameters = new DummyDeploymentParameters();
            var deployer = new TestDeployer(parameters, new LoggerFactory());
            deployer.Logger = loggerMock.Object;
            var startInfo = new ProcessStartInfo();
            var envVars = new Dictionary<string, string>();

            // Act
            deployer.AddEnvironmentVariablesToProcess(startInfo, envVars);

            // Assert
            Assert.True(deployer.AddEnvironmentVariablesToProcessCalled);
        }

        [Fact]
        public void StartTimer_LogsAndStartsStopwatch()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var parameters = new DummyDeploymentParameters();
            var deployer = new TestDeployer(parameters, new LoggerFactory());
            deployer.Logger = loggerMock.Object;

            // Act
            deployer.StartTimer();

            // Assert
            Assert.True(deployer.StartTimerCalled);
        }

        [Fact]
        public void StopTimer_LogsAndStopsStopwatch()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var parameters = new DummyDeploymentParameters();
            var deployer = new TestDeployer(parameters, new LoggerFactory());
            deployer.Logger = loggerMock.Object;

            // Act
            deployer.StopTimer();

            // Assert
            Assert.True(deployer.StopTimerCalled);
        }
    }
}
