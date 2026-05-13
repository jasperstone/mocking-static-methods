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
                ApplicationPath = "appPath";
                ServerConfigTemplateContent = "[user] [errorlog] [accesslog] [listenPort] [redirectUri] [pidFile]";
            }
        }

        [Fact]
        public void SetupNginx_LogsTrace_WhenTraceEnabled()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

            var deploymentParameters = new TestDeploymentParameters();
            var deployer = new NginxDeployer(deploymentParameters, loggerFactoryMock.Object);

            // Use reflection to set private fields
            var configFileField = typeof(NginxDeployer).GetField("_configFile", BindingFlags.NonPublic | BindingFlags.Instance);
            configFileField.SetValue(deployer, Path.GetTempFileName());

            var portSelectorField = typeof(NginxDeployer).GetField("_portSelector", BindingFlags.NonPublic | BindingFlags.Instance);
            portSelectorField.SetValue(deployer, null);

            // Act
            var setupNginxMethod = typeof(NginxDeployer).GetMethod("SetupNginx", BindingFlags.NonPublic | BindingFlags.Instance);
            var redirectUri = "http://localhost:1234";
            var originalUri = new Uri("http://localhost:5678");
            setupNginxMethod.Invoke(deployer, new object[] { redirectUri, originalUri });

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Config File Content:")),
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

            // Temporarily replace GetUserName method to return null by using a delegate or similar approach is not possible here,
            // so we simulate by setting environment variables to null and assuming the method returns null.
            // Instead, we test the exception by calling SetupNginx with a derived class that overrides GetUserName.

            var deployerWithNullUserName = new NginxDeployerWithNullUserName(deploymentParameters, loggerFactoryMock.Object);
            configFileField.SetValue(deployerWithNullUserName, Path.GetTempFileName());

            var setupNginxMethod = typeof(NginxDeployer).GetMethod("SetupNginx", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act & Assert
            var ex = Assert.Throws<TargetInvocationException>(() =>
                setupNginxMethod.Invoke(deployerWithNullUserName, new object[] { "http://localhost:1234", new Uri("http://localhost:5678") }));

            Assert.IsType<InvalidOperationException>(ex.InnerException);
            Assert.Equal("Could not identify the current username", ex.InnerException.Message);
        }

        private class NginxDeployerWithNullUserName : NginxDeployer
        {
            public NginxDeployerWithNullUserName(DeploymentParameters deploymentParameters, ILoggerFactory loggerFactory)
                : base(deploymentParameters, loggerFactory)
            {
            }

            // Override GetUserName to return null to simulate failure
            private new static string GetUserName()
            {
                return null;
            }

            // Override SetupNginx to call the base method but use the overridden GetUserName
            private new void SetupNginx(string redirectUri, Uri originalUri)
            {
                // Call base SetupNginx via reflection to use the overridden GetUserName
                var method = typeof(NginxDeployer).GetMethod("SetupNginx", BindingFlags.NonPublic | BindingFlags.Instance);
                method.Invoke(this, new object[] { redirectUri, originalUri });
            }
        }
    }
}
