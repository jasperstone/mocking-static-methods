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
            public TestDeploymentParameters() : base("appPath", "appName", ServerType.Nginx, RuntimeFlavor.CoreClr, ApplicationType.Portable, new Mock<ILoggerFactory>().Object)
            {
                ApplicationPath = Path.GetTempPath();
                ServerConfigTemplateContent = "[user][errorlog][accesslog][listenPort][redirectUri][pidFile]";
            }
        }

        [Fact]
        public void SetupNginx_LogsTrace_WhenTraceEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var deploymentParameters = new TestDeploymentParameters();
            deploymentParameters.ApplicationPath = Path.GetTempPath();
            deploymentParameters.ServerConfigTemplateContent = "[user]\n[errorlog]\n[accesslog]\n[listenPort]\n[redirectUri]\n[pidFile]";

            var deployer = new NginxDeployer(deploymentParameters, loggerFactoryMock.Object);

            // Use reflection to set private field _configFile to a temp file path
            var configFileField = typeof(NginxDeployer).GetField("_configFile", BindingFlags.NonPublic | BindingFlags.Instance);
            var tempConfigFile = Path.GetTempFileName();
            configFileField.SetValue(deployer, tempConfigFile);

            // Use reflection to set private field _portSelector to null to test the branch without reuseport
            var portSelectorField = typeof(NginxDeployer).GetField("_portSelector", BindingFlags.NonPublic | BindingFlags.Instance);
            portSelectorField.SetValue(deployer, null);

            // Act
            // SetupNginx is private, invoke via reflection
            var method = typeof(NginxDeployer).GetMethod("SetupNginx", BindingFlags.NonPublic | BindingFlags.Instance);
            var redirectUri = "http://localhost:1234";
            var originalUri = new Uri("http://localhost:5678");
            method.Invoke(deployer, new object[] { redirectUri, originalUri });

            // Assert
            // Verify LogTrace was called with expected message containing "Config File Content"
            loggerMock.Verify(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Config File Content")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Cleanup temp file
            if (File.Exists(tempConfigFile))
            {
                File.Delete(tempConfigFile);
            }
        }
    }
}
