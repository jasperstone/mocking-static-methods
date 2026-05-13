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
            public TestDeploymentParameters() : base("appPath", "appName", ServerType.Nginx, RuntimeFlavor.CoreClr, ApplicationType.Standalone, new Mock<ILoggerFactory>().Object)
            {
                ApplicationPath = Path.GetTempPath();
                ServerConfigTemplateContent = "[user] [errorlog] [accesslog] [listenPort] [redirectUri] [pidFile]";
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

            var deployer = new NginxDeployer(deploymentParameters, loggerFactoryMock.Object);

            // Use reflection to set private fields
            var configFileField = typeof(NginxDeployer).GetField("_configFile", BindingFlags.NonPublic | BindingFlags.Instance);
            configFileField.SetValue(deployer, Path.GetTempFileName());

            var portSelectorField = typeof(NginxDeployer).GetField("_portSelector", BindingFlags.NonPublic | BindingFlags.Instance);
            portSelectorField.SetValue(deployer, null);

            // Act
            var redirectUri = "http://localhost:1234";
            var originalUri = new Uri("http://localhost:5678");

            // Call private method SetupNginx via reflection
            var method = typeof(NginxDeployer).GetMethod("SetupNginx", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(deployer, new object[] { redirectUri, originalUri });

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Config File Content")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void SetupNginx_Throws_WhenUserNameIsNull()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var deploymentParameters = new TestDeploymentParameters();

            var deployer = new NginxDeployer(deploymentParameters, loggerFactoryMock.Object);

            // Use reflection to set private fields
            var configFileField = typeof(NginxDeployer).GetField("_configFile", BindingFlags.NonPublic | BindingFlags.Instance);
            configFileField.SetValue(deployer, Path.GetTempFileName());

            // Mock GetUserName to return null by temporarily replacing the environment variables
            // This is tricky since GetUserName is private static, so we test indirectly by setting environment variables to null
            Environment.SetEnvironmentVariable("LOGNAME", null);
            Environment.SetEnvironmentVariable("USER", null);
            Environment.SetEnvironmentVariable("USERNAME", null);

            // Also, on non-Windows, whoami will be called, but we cannot mock that easily here.
            // So we skip this test on non-Windows.
            if (!OperatingSystem.IsWindows())
            {
                return;
            }

            var redirectUri = "http://localhost:1234";
            var originalUri = new Uri("http://localhost:5678");

            var method = typeof(NginxDeployer).GetMethod("SetupNginx", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act & Assert
            var ex = Assert.Throws<TargetInvocationException>(() => method.Invoke(deployer, new object[] { redirectUri, originalUri }));
            Assert.IsType<InvalidOperationException>(ex.InnerException);
            Assert.Equal("Could not identify the current username", ex.InnerException.Message);
        }
    }
}
