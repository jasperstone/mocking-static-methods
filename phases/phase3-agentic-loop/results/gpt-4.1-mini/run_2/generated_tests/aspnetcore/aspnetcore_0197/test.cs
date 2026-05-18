using System;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class NginxDeployerTests
    {
        [Fact]
        public void SetupNginx_LogsTrace_WhenTraceEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var deploymentParameters = new DeploymentParameters
            {
                ApplicationPath = Path.GetTempPath(),
                ServerConfigTemplateContent = "[user][errorlog][accesslog][listenPort][redirectUri][pidFile]"
            };

            var deployer = new TestNginxDeployer(deploymentParameters, loggerFactoryMock.Object);

            // Setup logger to enable Trace level
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

            // Setup Logger.BeginScope to return a disposable
            loggerMock.Setup(l => l.BeginScope(It.IsAny<string>())).Returns(Mock.Of<IDisposable>());

            // Act
            var originalUri = new Uri("http://localhost:1234");
            var redirectUri = "http://redirect/";
            deployer.InvokeSetupNginx(redirectUri, originalUri);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Config File Content")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class TestNginxDeployer : NginxDeployer
        {
            public TestNginxDeployer(DeploymentParameters deploymentParameters, ILoggerFactory loggerFactory)
                : base(deploymentParameters, loggerFactory)
            {
                // Override _configFile to a temp file to avoid file write issues
                typeof(NginxDeployer).GetField("_configFile", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .SetValue(this, Path.GetTempFileName());
            }

            public void InvokeSetupNginx(string redirectUri, Uri originalUri)
            {
                // Call the private SetupNginx method via reflection
                var method = typeof(NginxDeployer).GetMethod("SetupNginx", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method.Invoke(this, new object[] { redirectUri, originalUri });
            }
        }
    }
}
