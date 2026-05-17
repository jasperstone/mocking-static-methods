using System;
using System.Globalization;
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
        [Fact]
        public void SetupNginx_LogsTraceWithConfigContent_WhenTraceEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var deploymentParameters = new DeploymentParameters
            {
                ApplicationPath = Path.GetTempPath(),
                ServerConfigTemplateContent = "[user] [errorlog] [accesslog] [listenPort] [redirectUri] [pidFile]"
            };

            var deployer = new NginxDeployer(deploymentParameters, loggerFactoryMock.Object);

            // Setup logger to enable Trace level
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

            // Setup Logger.BeginScope to return a disposable
            loggerMock.Setup(l => l.BeginScope(It.IsAny<string>())).Returns(Mock.Of<IDisposable>());

            // Set environment variable to simulate GetUserName returning a value
            Environment.SetEnvironmentVariable("LOGNAME", "testuser");

            // Setup private fields via reflection
            var configFileField = typeof(NginxDeployer).GetField("_configFile", BindingFlags.NonPublic | BindingFlags.Instance);
            var configFile = Path.GetTempFileName();
            configFileField.SetValue(deployer, configFile);

            var portSelectorField = typeof(NginxDeployer).GetField("_portSelector", BindingFlags.NonPublic | BindingFlags.Instance);
            portSelectorField.SetValue(deployer, null);

            var originalUri = new Uri("http://localhost:1234");

            // Act
            var setupNginxMethod = typeof(NginxDeployer).GetMethod("SetupNginx", BindingFlags.NonPublic | BindingFlags.Instance);
            setupNginxMethod.Invoke(deployer, new object[] { "http://redirect", originalUri });

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Config File Content:") && v.ToString().Contains("===START CONFIG===") && v.ToString().Contains("===END CONFIG===")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Cleanup
            Environment.SetEnvironmentVariable("LOGNAME", null);
            if (File.Exists(configFile))
            {
                File.Delete(configFile);
            }
        }
    }
}
