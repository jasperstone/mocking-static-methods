using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.AspNetCore.Server.IntegrationTesting.Common;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class NginxDeployerTests
    {
        [Fact]
        public void SetupNginx_LogsTrace_WhenTraceIsEnabled()
        {
            // Arrange
            var logger = new ListLogger();
            logger.IsEnabledSetup(LogLevel.Trace, true);
            
            var deploymentParameters = new DeploymentParameters
            {
                ApplicationPath = "/tmp/app",
                ServerConfigTemplateContent = "[user][errorlog][accesslog][listenPort][redirectUri][pidFile]content"
            };
            
            var deployer = new TestableNginxDeployer(deploymentParameters, logger.MockFactory.Object);

            // Act
            deployer.SetupNginx("http://example.com:5000", new Uri("http://localhost:8080"));

            // Assert
            var traceMessage = logger.Messages.Find(m => m.Contains("Config File Content"));
            Assert.NotNull(traceMessage);
            Assert.Contains("===START CONFIG===", traceMessage);
            Assert.Contains("content", traceMessage);
            Assert.Contains("===END CONFIG===", traceMessage);
        }

        [Fact]
        public void SetupNginx_DoesNotLogTrace_WhenTraceIsDisabled()
        {
            // Arrange
            var logger = new ListLogger();
            logger.IsEnabledSetup(LogLevel.Trace, false);
            
            var deploymentParameters = new DeploymentParameters
            {
                ApplicationPath = "/tmp/app",
                ServerConfigTemplateContent = "some content"
            };
            
            var deployer = new TestableNginxDeployer(deploymentParameters, logger.MockFactory.Object);

            // Act
            deployer.SetupNginx("http://example.com:5000", new Uri("http://localhost:8080"));

            // Assert
            Assert.DoesNotContain(logger.Messages, m => m.Contains("===START CONFIG==="));
        }
    }

    public class ListLogger : ILogger
    {
        public Mock<ILogger> MockLogger { get; } = new Mock<ILogger>();
        public Mock<ILoggerFactory> MockFactory { get; } = new Mock<ILoggerFactory>();
        public List<string> Messages { get; } = new List<string>();

        public ListLogger()
        {
            MockFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(MockLogger.Object);
            MockLogger.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            MockLogger.Setup(l => l.Log(
                It.Is<LogLevel>(l => l == LogLevel.Trace),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Callback<LogLevel, EventId, object, Exception, Func<object, Exception?, string>>(
                    (level, id, state, ex, formatter) => Messages.Add(formatter(state, ex)));
        }

        public void IsEnabledSetup(LogLevel level, bool enabled)
        {
            MockLogger.Setup(l => l.IsEnabled(level)).Returns(enabled);
        }

        public IDisposable? BeginScope<TState>(TState state) => NullScope.Instance;
        public bool IsEnabled(LogLevel logLevel) => MockLogger.Object.IsEnabled(logLevel);
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => MockLogger.Object.Log(logLevel, eventId, state, exception, formatter);
    }

    public class TestableNginxDeployer : NginxDeployer
    {
        private readonly string _fakeUserName = "testuser";

        public TestableNginxDeployer(DeploymentParameters deploymentParameters, ILoggerFactory loggerFactory)
            : base(deploymentParameters, loggerFactory)
        {
        }

        public new void SetupNginx(string redirectUri, Uri originalUri)
        {
            // Override problematic operations to make test run
            _configFile = Path.GetTempFileName();
            
            // Mock static GetUserName via reflection replacement or just provide the data
            DeploymentParameters.ServerConfigTemplateContent ??= "[user][errorlog][accesslog][listenPort][redirectUri][pidFile]";
            
            base.SetupNginx(redirectUri, originalUri);
        }

        private static new string GetUserName() => "testuser";
    }
}
