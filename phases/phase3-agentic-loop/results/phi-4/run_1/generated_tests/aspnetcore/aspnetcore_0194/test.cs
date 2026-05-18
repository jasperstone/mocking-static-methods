using System;
using System.IO;
using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class NginxDeployerTests
    {
        [Fact]
        public void SetupNginx_LogsDebugMessagesForPidErrorAndAccessLogs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var deploymentParametersMock = new Mock<DeploymentParameters>();
            deploymentParametersMock.SetupGet(p => p.ApplicationPath).Returns(Path.GetTempPath());
            deploymentParametersMock.SetupGet(p => p.ServerConfigTemplateContent).Returns("[user]\n[errorlog]\n[accesslog]\n[listenPort]\n[redirectUri]\n[pidFile]");

            var nginxDeployer = new NginxDeployer(deploymentParametersMock.Object, loggerMock.Object);

            // Act
            nginxDeployer.SetupNginx("http://localhost", new Uri("http://localhost:5000"));

            // Assert
            loggerMock.Verify(
                x => x.LogDebug("Using PID file: {pidFile}", It.IsAny<string>()),
                Times.Once);

            loggerMock.Verify(
                x => x.LogDebug("Using Error Log file: {errorLog}", It.IsAny<string>()),
                Times.Once);

            loggerMock.Verify(
                x => x.LogDebug("Using Access Log file: {accessLog}", It.IsAny<string>()),
                Times.Once);
        }
    }
}
