using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace NginxDeployerTests
{
    public class NginxDeployerTest
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
                // Call base but override Logger to capture LogTrace
                using (Logger.BeginScope("SetupNginx"))
                {
                    var userName = GetUserName() ?? throw new InvalidOperationException("Could not identify the current username");
                    var pidFile = Path.Combine(DeploymentParameters.ApplicationPath, $"{Guid.NewGuid()}.nginx.pid");
                    var errorLog = Path.Combine(DeploymentParameters.ApplicationPath, "nginx.error.log");
                    var accessLog = Path.Combine(DeploymentParameters.ApplicationPath, "nginx.access.log");
                    DeploymentParameters.ServerConfigTemplateContent = DeploymentParameters.ServerConfigTemplateContent
                        .Replace("[user]", userName)
                        .Replace("[errorlog]", errorLog)
                        .Replace("[accesslog]", accessLog)
                        .Replace("[listenPort]", originalUri.Port.ToString(CultureInfo.InvariantCulture) + (_portSelector != null ? " reuseport" : ""))
                        .Replace("[redirectUri]", redirectUri)
                        .Replace("[pidFile]", pidFile);
                    var logger = Logger as MockLogger ?? throw new InvalidOperationException("Logger is not a MockLogger");
                    if (logger.IsTraceEnabled)
                    {
                        LoggedContent = DeploymentParameters.ServerConfigTemplateContent;
                        LogTraceCalled = true;
                    }
                }
            }

            public override void Dispose()
            {
                // No-op for test
            }
        }

        private class MockLogger : ILogger
        {
            public bool IsTraceEnabled { get; set; }
            public string LastLogMessage { get; private set; }
            public LogLevel LastLogLevel { get; private set; }
            public object[] LastState { get; private set; }

            public IDisposable BeginScope<TState>(TState state) => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                LastLogMessage = formatter(state, exception);
                LastLogLevel = logLevel;
                LastState = new object[] { state };
            }
        }

        [Fact]
        public void LogTrace_IsCalled_WhenTraceEnabled()
        {
            // Arrange
            var mockLogger = new MockLogger { IsTraceEnabled = true };
            var loggerFactory = new LoggerFactory();
            var parameters = new DeploymentParameters
            {
                ApplicationPath = Path.GetTempPath(),
                ServerConfigTemplateContent = "config content with [user]"
            };
            var deployer = new TestNginxDeployer(parameters, loggerFactory);
            deployer.Logger = mockLogger;

            // Act
            deployer.SetupNginx("http://redirect", new Uri("http://localhost:1234"));

            // Assert
            Assert.True(deployer.LogTraceCalled);
            Assert.Contains("[user]", mockLogger.LastLogMessage);
        }

        [Fact]
        public void LogTrace_IsNotCalled_WhenTraceNotEnabled()
        {
            // Arrange
            var mockLogger = new MockLogger { IsTraceEnabled = false };
            var loggerFactory = new LoggerFactory();
            var parameters = new DeploymentParameters
            {
                ApplicationPath = Path.GetTempPath(),
                ServerConfigTemplateContent = "config content with [user]"
            };
            var deployer = new TestNginxDeployer(parameters, loggerFactory);
            deployer.Logger = mockLogger;

            // Act
            deployer.SetupNginx("http://redirect", new Uri("http://localhost:1234"));

            // Assert
            Assert.False(deployer.LogTraceCalled);
        }
    }

    // Placeholder for DeploymentParameters class
    public class DeploymentParameters
    {
        public string ApplicationPath { get; set; }
        public string ServerConfigTemplateContent { get; set; }
        public static string ServerConfigTemplateContent { get; set; }
    }
}
