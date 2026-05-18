using System;
using System.IO;
using Microsoft.AspNetCore.Server.IntegrationTesting.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class NginxDeployerTests
    {
        [Fact]
        public void SetupNginx_LogsDebugMessagesWithCorrectArguments()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NginxDeployer>>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(false);

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger("Microsoft.AspNetCore.Server.IntegrationTesting.NginxDeployer"))
                           .Returns(loggerMock.Object);

            var deploymentParameters = new DeploymentParameters
            {
                ApplicationPath = Path.GetTempPath(),
                ServerConfigTemplateContent = "template"
            };

            var deployer = new TestableNginxDeployer(deploymentParameters, loggerFactoryMock.Object);
            deployer.SetGetUserNameReturnValue("testuser");

            // Act
            deployer.SetupNginx("http://localhost:5001", new Uri("http://localhost:5000"));

            // Assert - verify the LogDebug extension method calls
            loggerMock.Verify(
                x => x.LogDebug(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    "Using PID file: {pidFile}",
                    It.IsAny<string>()),
                Times.Once);

            loggerMock.Verify(
                x => x.LogDebug(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    "Using Error Log file: {errorLog}",
                    It.IsAny<string>()),
                Times.Once);

            loggerMock.Verify(
                x => x.LogDebug(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    "Using Access Log file: {accessLog}",
                    It.IsAny<string>()),
                Times.Once);
        }
    }

    public class TestableNginxDeployer : NginxDeployer
    {
        private Func<string> _getUserNameDelegate = () => "testuser";

        public TestableNginxDeployer(DeploymentParameters deploymentParameters, ILoggerFactory loggerFactory)
            : base(deploymentParameters, loggerFactory)
        {
        }

        public void SetGetUserNameReturnValue(string value)
        {
            _getUserNameDelegate = () => value;
        }

        protected override string GetUserName() => _getUserNameDelegate();

        public new void SetupNginx(string redirectUri, Uri originalUri) => base.SetupNginx(redirectUri, originalUri);
    }
}
