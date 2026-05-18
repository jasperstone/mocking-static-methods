using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class DeploymentParameters
    {
        public string EnvironmentName { get; set; }
        public string ApplicationPath { get; set; }
        public string ApplicationName { get; set; }
        public ServerType ServerType { get; set; }
        public RuntimeFlavor RuntimeFlavor { get; set; }
        public string TargetFramework { get; set; }
        public RuntimeArchitecture RuntimeArchitecture { get; set; }
        public Action<DeploymentParameters> UserAdditionalCleanup { get; set; }
    }

    public enum ServerType
    {
        None,
        Kestrel,
        HttpSys
    }

    public enum RuntimeFlavor
    {
        None,
        CoreClr,
        Clr
    }

    public enum RuntimeArchitecture
    {
        x86,
        x64
    }

    public class DeploymentResult
    {
    }

    public class ApplicationDeployerTests : ApplicationDeployer
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;

        public ApplicationDeployerTests() : base(new DeploymentParameters(), new LoggerFactory())
        {
            _loggerMock = new Mock<ILogger>();
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(_loggerMock.Object);
            LoggerFactory = _loggerFactoryMock.Object;
            Logger = _loggerMock.Object;
        }

        public override Task<DeploymentResult> DeployAsync()
        {
            return Task.FromResult(new DeploymentResult());
        }

        public override void Dispose()
        {
        }

        [Fact]
        public void TriggerHostShutdown_LogsInformation()
        {
            // Arrange
            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            TriggerHostShutdown(cancellationTokenSource);

            // Assert
            _loggerMock.Verify(x => x.LogInformation(It.IsAny<string>()), Times.Once);
        }
    }
}
