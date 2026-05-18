using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace NginxDeployerTests
{
    public class NginxDeployerLoggingTests
    {
        private class TestNginxDeployer : NginxDeployer
        {
            public StringWriter LogOutput { get; } = new StringWriter();

            public TestNginxDeployer(DeploymentParameters parameters, ILoggerFactory loggerFactory)
                : base(parameters, loggerFactory)
            {
            }

            public override void Dispose()
            {
                base.Dispose();
            }
        }

        [Fact]
        public void SetupNginx_LogsDebugMessages_WithCorrectPidFile()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var parameters = new DeploymentParameters
            {
                ApplicationPath = Path.GetTempPath(),
                ServerConfigTemplateContent = "config content with [user], [errorlog], [accesslog], [listenPort], [redirectUri], [pidFile]",
            };

            var deployer = new TestNginxDeployer(parameters, mockLoggerFactory.Object);

            // Setup environment for GetUserName
            Environment.SetEnvironmentVariable("LOGNAME", "testuser");
            Environment.SetEnvironmentVariable("USER", null);
            Environment.SetEnvironmentVariable("USERNAME", null);

            // Act
            deployer.SetupNginx("http://redirect", new Uri("http://localhost:5000"));

            // Assert
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Using PID file:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(3));
        }
    }
}
