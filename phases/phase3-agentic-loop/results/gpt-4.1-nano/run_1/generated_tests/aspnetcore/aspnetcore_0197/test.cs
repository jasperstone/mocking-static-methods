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
            public bool LogTraceCalled { get; private set; }
            public string LoggedMessage { get; private set; }
            public object LoggedContent { get; private set; }

            public TestNginxDeployer(DeploymentParameters parameters, ILoggerFactory loggerFactory)
                : base(parameters, loggerFactory)
            {
            }

            protected override void SetupNginx(string redirectUri, Uri originalUri)
            {
                // Create a mock logger to intercept LogTrace
                var mockLogger = new Mock<ILogger>();
                mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
                mockLogger.Setup(l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
                    .Callback<LogLevel, EventId, object, Exception, Delegate>((level, eventId, state, exception, formatter) =>
                    {
                        if (level == LogLevel.Trace)
                        {
                            LogTraceCalled = true;
                            LoggedMessage = state.ToString();
                            LoggedContent = state;
                        }
                    });

                // Replace the Logger with our mock
                Logger = mockLogger.Object;

                // Call the original method logic
                base.SetupNginx(redirectUri, originalUri);
            }
        }

        [Fact]
        public void SetupNginx_ShouldCallLogTrace_WhenTraceEnabled()
        {
            // Arrange
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var parameters = new DeploymentParameters
            {
                ApplicationPath = Path.GetTempPath(),
                ServerConfigTemplateContent = "Test config with [user], [errorlog], [accesslog], [listenPort], [redirectUri], [pidFile]"
            };

            var deployer = new TestNginxDeployer(parameters, mockLoggerFactory.Object);

            // Act
            deployer.SetupNginx("http://redirect", new Uri("http://localhost:5000"));

            // Assert
            Assert.True(deployer.LogTraceCalled, "LogTrace should be called when Trace is enabled");
            Assert.Contains("Config File Content:", deployer.LoggedMessage);
            Assert.NotNull(deployer.LoggedContent);
        }
    }
}
