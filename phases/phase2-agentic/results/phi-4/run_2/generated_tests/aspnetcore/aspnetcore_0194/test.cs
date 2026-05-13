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
        public void SetupNginx_LogsDebugMessagesForPidFile()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NginxDeployer>>();
            var deploymentParameters = new DeploymentParameters
            {
                ApplicationPath = Path.GetTempPath(),
                ServerConfigTemplateContent = "[user] [errorlog] [accesslog] [listenPort] [redirectUri] [pidFile]"
            };

            var nginxDeployer = new NginxDeployer(deploymentParameters, new LoggerFactory().AddProvider(new MockProvider(loggerMock.Object)));

            var redirectUri = "http://localhost";
            var originalUri = new Uri("http://localhost:5000");

            // Act
            nginxDeployer.SetupNginx(redirectUri, originalUri);

            // Assert
            loggerMock.Verify(
                x => x.LogDebug(
                    It.Is<string>(s => s.Contains("Using PID file:")),
                    It.Is<object[]>(o => o.Length == 1 && o[0] is string pidFile && File.Exists(pidFile))
                ),
                Times.Once
            );
        }
    }

    public class MockProvider : ILoggerProvider
    {
        private readonly Mock<ILogger> _loggerMock;

        public MockProvider(Mock<ILogger> loggerMock)
        {
            _loggerMock = loggerMock;
        }

        public ILogger CreateLogger(string categoryName) => _loggerMock.Object;

        public void Dispose() { }
    }
}
