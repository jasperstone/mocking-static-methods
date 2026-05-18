using System;
using System.Threading;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    // Minimal stub for DeploymentParameters to allow construction of ApplicationDeployer
    public class DeploymentParameters
    {
        public ServerType ServerType { get; set; }
        public RuntimeFlavor RuntimeFlavor { get; set; }
        public string ApplicationPath { get; set; }
        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; }
        public bool PreservePublishedApplicationForDebugging { get; set; }
        public Action<DeploymentParameters> UserAdditionalCleanup { get; set; }
        public string TargetFramework { get; set; }
        public RuntimeArchitecture RuntimeArchitecture { get; set; }
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
        CoreClr,
        Clr
    }

    public enum RuntimeArchitecture
    {
        X64,
        X86
    }

    public class ApplicationPublisher
    {
        public ApplicationPublisher(string path) { }
        public System.Threading.Tasks.Task<PublishedApplication> Publish(DeploymentParameters parameters, ILogger logger) =>
            System.Threading.Tasks.Task.FromResult(new PublishedApplication());
    }

    public class PublishedApplication : IDisposable
    {
        public string Path { get; set; } = "publishedPath";
        public void Dispose() { }
    }

    public class DeploymentResult { }

    public class TestDeployer : ApplicationDeployer
    {
        public TestDeployer(DeploymentParameters deploymentParameters, ILoggerFactory loggerFactory)
            : base(deploymentParameters, loggerFactory)
        {
        }

        public override void Dispose()
        {
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

    public class ApplicationDeployerTests
    {
        [Fact]
        public void TriggerHostShutdown_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var deploymentParameters = new DeploymentParameters
            {
                ServerType = ServerType.Kestrel,
                RuntimeFlavor = RuntimeFlavor.CoreClr,
                ApplicationPath = "somepath"
            };

            var deployer = new TestDeployer(deploymentParameters, loggerFactoryMock.Object);
            var cts = new CancellationTokenSource();

            // Act
            deployer.CallTriggerHostShutdown(cts);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Host process shutting down.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
