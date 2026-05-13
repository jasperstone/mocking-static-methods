using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class NginxDeployerTests
    {
        private class TestDeploymentParameters : DeploymentParameters
        {
            public TestDeploymentParameters()
                : base("appPath", "appName", ServerType.Nginx, RuntimeFlavor.CoreClr, RuntimeArchitecture.x64, ApplicationType.Portable)
            {
                ApplicationPath = Path.GetTempPath();
                ServerConfigTemplateContent = "[user] [errorlog] [accesslog] [listenPort] [redirectUri] [pidFile]";
            }
        }

        [Fact]
        public void SetupNginx_LogsDebugMessagesWithCorrectPidFile()
        {
            // Arrange
            var deploymentParameters = new TestDeploymentParameters();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var deployer = new NginxDeployer(deploymentParameters, loggerFactoryMock.Object);

            // Use reflection to get the private SetupNginx method
            var method = typeof(NginxDeployer).GetMethod("SetupNginx", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(method);

            // Setup logger to always return true for IsEnabled(LogLevel.Trace)
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);

            // Act
            // We call SetupNginx with dummy redirectUri and originalUri with port 1234
            var redirectUri = "http://localhost:1234";
            var originalUri = new Uri("http://localhost:1234");

            // We need to set DeploymentParameters.ApplicationPath to a temp directory to avoid file write issues
            deploymentParameters.ApplicationPath = Path.GetTempPath();

            // Call SetupNginx
            method.Invoke(deployer, new object[] { redirectUri, originalUri });

            // Assert
            // Capture the pidFile argument from the first LogDebug call
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Using PID file:")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            // Verify the other two LogDebug calls with errorLog and accessLog
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Using Error Log file:")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Using Access Log file:")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}
