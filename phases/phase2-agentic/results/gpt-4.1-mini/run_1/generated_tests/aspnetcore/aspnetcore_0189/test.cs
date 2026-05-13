using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.Extensions.Logging;
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
                // No-op for test
            }

            public override System.Threading.Tasks.Task<DeploymentResult> DeployAsync()
            {
                throw new NotImplementedException();
            }

            public void CallTriggerHostShutdown(CancellationTokenSource cts)
            {
                TriggerHostShutdown(cts);
            }
        }

        [Fact]
        public void TriggerHostShutdown_LogsInformationAndCancelsToken()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var deploymentParameters = new DeploymentParameters
            {
                ServerType = ServerType.Kestrel,
                EnvironmentName = "Development"
            };

            var deployer = new TestApplicationDeployer(deploymentParameters, loggerFactoryMock.Object);

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
    }

    // Minimal stubs for required types
    public class DeploymentParameters
    {
        public ServerType ServerType { get; set; }
        public string EnvironmentName { get; set; }
        public Action<DeploymentParameters> UserAdditionalCleanup { get; set; }
        public string ApplicationPath { get; set; }
        public string ApplicationName { get; set; }
        public string TargetFramework { get; set; }
        public RuntimeFlavor RuntimeFlavor { get; set; }
        public RuntimeArchitecture RuntimeArchitecture { get; set; }
        public bool PreservePublishedApplicationForDebugging { get; set; }
        public ApplicationPublisher ApplicationPublisher { get; set; }
        public string PublishedApplicationRootPath { get; set; }
    }

    public enum ServerType
    {
        None,
        Kestrel
    }

    public enum RuntimeFlavor
    {
        None,
        Clr,
        CoreClr
    }

    public enum RuntimeArchitecture
    {
        X64,
        X86
    }

    public class DeploymentResult : IDisposable
    {
        public void Dispose()
        {
        }
    }

    public class ApplicationPublisher
    {
        private readonly string _applicationPath;

        public ApplicationPublisher(string applicationPath)
        {
            _applicationPath = applicationPath;
        }

        public System.Threading.Tasks.Task<PublishedApplication> Publish(DeploymentParameters parameters, ILogger logger)
        {
            return System.Threading.Tasks.Task.FromResult(new PublishedApplication(_applicationPath));
        }
    }

    public class PublishedApplication : IDisposable
    {
        public string Path { get; }

        public PublishedApplication(string path)
        {
            Path = path;
        }

        public void Dispose()
        {
        }
    }
}
