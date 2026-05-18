using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class NginxDeployerTests
    {
        [Fact]
        public void SetupNginx_LogsDebugMessagesForPidErrorAndAccessLogs()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var mockLogger = new Mock<ILogger>();
            loggerFactory.AddProvider(new MockLoggerProvider(mockLogger.Object));

            var deploymentParameters = new DeploymentParameters
            {
                ApplicationPath = Path.GetTempPath(),
                ServerConfigTemplateContent = "server {user} {errorlog} {accesslog} {listenPort} {redirectUri} {pidFile};"
            };

            var nginxDeployer = new NginxDeployer(deploymentParameters, loggerFactory);

            var redirectUri = "http://localhost";
            var originalUri = new Uri("http://localhost:5000");

            // Act
            nginxDeployer.SetupNginx(redirectUri, originalUri);

            // Assert
            mockLogger.Verify(
                x => x.LogDebug("Using PID file: {pidFile}", It.IsAny<string>()),
                Times.Once);

            mockLogger.Verify(
                x => x.LogDebug("Using Error Log file: {errorLog}", It.IsAny<string>()),
                Times.Once);

            mockLogger.Verify(
                x => x.LogDebug("Using Access Log file: {accessLog}", It.IsAny<string>()),
                Times.Once);
        }
    }

    public class MockLoggerProvider : ILoggerProvider
    {
        private readonly Mock<ILogger> _logger;

        public MockLoggerProvider(Mock<ILogger> logger)
        {
            _logger = logger;
        }

        public ILogger CreateLogger(string categoryName) => _logger.Object;

        public void Dispose() { }
    }

    // Mock DeploymentParameters class for testing purposes
    public class DeploymentParameters
    {
        public string ApplicationPath { get; set; }
        public string ServerConfigTemplateContent { get; set; }
    }
}
