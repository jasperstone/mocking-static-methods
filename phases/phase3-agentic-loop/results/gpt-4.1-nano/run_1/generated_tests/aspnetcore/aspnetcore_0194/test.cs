using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Server.IntegrationTesting;

namespace NginxDeployerTests
{
    public class NginxDeployerLoggingTests
    {
        private class DummyLogger : ILogger
        {
            public List<(LogLevel level, string message, object[] args)> Logs { get; } = new List<(LogLevel, string, object[])>();
            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                Logs.Add((logLevel, formatter(state, exception), null));
            }
        }

        [Fact]
        public void SetupNginx_LogsDebugMessagesIncludingPidFile()
        {
            // Arrange
            var mockLogger = new DummyLogger();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger);

            var deploymentParameters = new DeploymentParameters
            {
                ApplicationPath = "/tmp/app",
                ServerConfigTemplateContent = "config content with placeholders"
            };

            var deployer = new NginxDeployer(deploymentParameters, mockLoggerFactory.Object);

            // Use reflection to invoke the private method SetupNginx
            var methodInfo = typeof(NginxDeployer).GetMethod("SetupNginx", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var redirectUri = "http://localhost:5000";
            var originalUri = new Uri("http://localhost:1234");
            methodInfo.Invoke(deployer, new object[] { redirectUri, originalUri });

            // Assert
            var debugLogs = mockLogger.Logs.FindAll(l => l.level == LogLevel.Debug);
            Assert.Contains(debugLogs, l => l.message.Contains("Using PID file:"));
            Assert.Contains(debugLogs, l => l.message.Contains("Using Error Log file:"));
            Assert.Contains(debugLogs, l => l.message.Contains("Using Access Log file:"));
        }
    }
}
