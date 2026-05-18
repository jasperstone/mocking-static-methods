using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Xunit;
using Microsoft.AspNetCore.Server.IntegrationTesting;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class NginxDeployerLoggerTests
    {
        [Fact]
        public void SetupNginx_LogsTrace_WhenTraceIsEnabled()
        {
            // Arrange
            var logger = new TestLogger(true);
            var deploymentParameters = new DeploymentParameters
            {
                ApplicationPath = "/tmp/app",
                ServerConfigTemplateContent = "template content"
            };
            var deployer = new TestableNginxDeployer(deploymentParameters, logger);

            // Act
            deployer.SetupNginx("http://redirect", new Uri("http://localhost:8080"));

            // Assert
            Assert.Contains("Config File Content", logger.Messages[3]);
            Assert.Contains("===START CONFIG===", logger.Messages[3]);
            Assert.Contains("template content", logger.Messages[3]);
            Assert.Contains("===END CONFIG===", logger.Messages[3]);
        }

        [Fact]
        public void SetupNginx_DoesNotLogTrace_WhenTraceIsDisabled()
        {
            // Arrange
            var logger = new TestLogger(false);
            var deploymentParameters = new DeploymentParameters
            {
                ApplicationPath = "/tmp/app",
                ServerConfigTemplateContent = "template content"
            };
            var deployer = new TestableNginxDeployer(deploymentParameters, logger);

            // Act
            deployer.SetupNginx("http://redirect", new Uri("http://localhost:8080"));

            // Assert
            Assert.DoesNotContain("===START CONFIG===", string.Join(" ", logger.Messages));
        }
    }

    public class TestLogger : ILogger
    {
        public List<string> Messages { get; } = new List<string>();
        private readonly bool _traceEnabled;

        public TestLogger(bool traceEnabled)
        {
            _traceEnabled = traceEnabled;
        }

        public IDisposable? BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel <= LogLevel.Debug || (logLevel == LogLevel.Trace && _traceEnabled);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }
    }

    public class TestableNginxDeployer : NginxDeployer
    {
        public TestableNginxDeployer(DeploymentParameters deploymentParameters, ILogger logger)
            : base(deploymentParameters, new TestLoggerFactory(logger))
        {
        }

        public new void SetupNginx(string redirectUri, Uri originalUri)
        {
            base.SetupNginx(redirectUri, originalUri);
        }
    }

    public class TestLoggerFactory : ILoggerFactory
    {
        private readonly ILogger _logger;

        public TestLoggerFactory(ILogger logger)
        {
            _logger = logger;
        }

        public void AddProvider(ILoggerProvider provider) { }

        public ILogger CreateLogger(string categoryName) => _logger;

        public void Dispose() { }
    }
}
