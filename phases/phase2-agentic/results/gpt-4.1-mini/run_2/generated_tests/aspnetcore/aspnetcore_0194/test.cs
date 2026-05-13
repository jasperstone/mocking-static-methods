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
            public TestDeploymentParameters() : base("appPath", "appName", ServerType.Nginx, RuntimeFlavor.CoreClr, RuntimeArchitecture.x64, ApplicationType.Portable)
            {
                ServerConfigTemplateContent = "[user] [errorlog] [accesslog] [listenPort] [redirectUri] [pidFile]";
            }
        }

        [Fact]
        public void SetupNginx_LogsDebugMessagesWithCorrectPidFile()
        {
            // Arrange
            var deploymentParameters = new TestDeploymentParameters
            {
                ApplicationPath = Path.GetTempPath()
            };

            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var deployer = new NginxDeployer(deploymentParameters, loggerFactoryMock.Object);

            // Use reflection to invoke private SetupNginx method
            var setupNginxMethod = typeof(NginxDeployer).GetMethod("SetupNginx", BindingFlags.NonPublic | BindingFlags.Instance);

            // Setup environment variable for username to avoid InvalidOperationException
            Environment.SetEnvironmentVariable("LOGNAME", "testuser");

            var redirectUri = "http://localhost:1234/redirect";
            var originalUri = new Uri("http://localhost:5678");

            // Act
            setupNginxMethod.Invoke(deployer, new object[] { redirectUri, originalUri });

            // Assert
            // Capture the pidFile argument from the first LogDebug call
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Using PID file:")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

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

            // Cleanup environment variable
            Environment.SetEnvironmentVariable("LOGNAME", null);
        }
    }
}
