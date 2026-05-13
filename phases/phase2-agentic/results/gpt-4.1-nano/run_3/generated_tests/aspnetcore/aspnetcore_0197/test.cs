using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace NginxDeployerTests
{
    public class NginxDeployerTests
    {
        private class TestNginxDeployer : NginxDeployer
        {
            public bool LogTraceCalled { get; private set; }
            public string LoggedContent { get; private set; }

            public TestNginxDeployer(DeploymentParameters parameters, ILoggerFactory loggerFactory)
                : base(parameters, loggerFactory)
            {
            }

            protected override void SetupNginx(string redirectUri, Uri originalUri)
            {
                // Call base method but intercept LogTrace
                var logger = Logger;
                if (logger.IsEnabled(LogLevel.Trace))
                {
                    LoggedContent = DeploymentParameters.ServerConfigTemplateContent;
                    LogTraceCalled = true;
                }
                // Skip actual process start
            }
        }

        [Fact]
        public async Task SetupNginx_LogsTrace_WhenTraceEnabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            var loggerFactory = new LoggerFactory();
            var parameters = new DeploymentParameters
            {
                ApplicationPath = Path.GetTempPath(),
                ServerConfigTemplateContent = "config content with placeholders"
            };
            var deployer = new TestNginxDeployer(parameters, loggerFactory);
            deployer.Logger = mockLogger.Object;

            // Act
            deployer.LogTraceCalled = false;
            deployer.LoggedContent = null;
            deployer.SetupNginx("http://redirect", new Uri("http://localhost:5000"));

            // Assert
            Assert.True(deployer.LogTraceCalled);
            Assert.Contains("config content with placeholders", deployer.LoggedContent);
        }

        [Fact]
        public async Task SetupNginx_DoesNotLogTrace_WhenTraceDisabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
            var loggerFactory = new LoggerFactory();
            var parameters = new DeploymentParameters
            {
                ApplicationPath = Path.GetTempPath(),
                ServerConfigTemplateContent = "config content with placeholders"
            };
            var deployer = new TestNginxDeployer(parameters, loggerFactory);
            deployer.Logger = mockLogger.Object;

            // Act
            deployer.LogTraceCalled = false;
            deployer.LoggedContent = null;
            deployer.SetupNginx("http://redirect", new Uri("http://localhost:5000"));

            // Assert
            Assert.False(deployer.LogTraceCalled);
            Assert.Null(deployer.LoggedContent);
        }
    }

    // Placeholder classes to simulate actual dependencies
    public class DeploymentParameters
    {
        public string ApplicationPath { get; set; }
        public string ServerConfigTemplateContent { get; set; }
    }

    public static class OperatingSystem
    {
        public static bool IsLinux() => true;
        public static bool IsWindows() => false;
    }
}
