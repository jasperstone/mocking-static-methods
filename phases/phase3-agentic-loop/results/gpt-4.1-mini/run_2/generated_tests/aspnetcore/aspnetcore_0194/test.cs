using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Server.IntegrationTesting;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class NginxDeployerTests
    {
        private class TestDeploymentParameters : DeploymentParameters
        {
            public TestDeploymentParameters()
            {
                ApplicationPath = Path.GetTempPath();
                ServerConfigTemplateContent = "[user][errorlog][accesslog][listenPort][redirectUri][pidFile]";
                ServerType = ServerType.Nginx;
                RuntimeFlavor = RuntimeFlavor.CoreClr;
                RuntimeArchitecture = RuntimeArchitecture.x64;
                ApplicationType = ApplicationType.Portable;
            }
        }

        [Fact]
        public void SetupNginx_LogsDebugMessagesIncludingPidFile()
        {
            // Arrange
            var deploymentParameters = new TestDeploymentParameters();
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var deployer = new NginxDeployer(deploymentParameters, loggerFactoryMock.Object);

            // Setup minimal required state
            var originalUri = new Uri("http://localhost:1234");
            var redirectUri = "http://redirect";

            // We need to set _configFile field because SetupNginx writes to it
            var configFileField = typeof(NginxDeployer).GetField("_configFile", BindingFlags.NonPublic | BindingFlags.Instance);
            var tempConfigFile = Path.GetTempFileName();
            configFileField.SetValue(deployer, tempConfigFile);

            // We need to set DeploymentParameters.ApplicationPath to a temp directory
            deploymentParameters.ApplicationPath = Path.GetTempPath();

            // We need to set _portSelector to null to test the branch without reuseport
            var portSelectorField = typeof(NginxDeployer).GetField("_portSelector", BindingFlags.NonPublic | BindingFlags.Instance);
            portSelectorField.SetValue(deployer, null);

            // Act
            deployer.SetupNginx(redirectUri, originalUri);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Using PID file:")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Using Error Log file:")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Using Access Log file:")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Cleanup
            if (File.Exists(tempConfigFile))
            {
                File.Delete(tempConfigFile);
            }
        }
    }
}
