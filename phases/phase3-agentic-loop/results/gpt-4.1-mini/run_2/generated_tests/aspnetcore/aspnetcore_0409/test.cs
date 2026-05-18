using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.NodeServices.Npm;
using Microsoft.AspNetCore.NodeServices.Util;
using Xunit;
using Moq;

namespace Microsoft.AspNetCore.NodeServices.Npm.Tests
{
    public class NodeScriptRunnerTests
    {
        [Fact]
        public void AttachToLogger_LogsErrorOnStdErrReceivedLine()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var diagnosticSource = new DiagnosticListener("Test");
            using var cts = new CancellationTokenSource();

            var runner = new TestNodeScriptRunner(
                Environment.CurrentDirectory,
                "testscript",
                null,
                null,
                "npm",
                diagnosticSource,
                cts.Token);

            string? loggedError = null;
            mockLogger.Setup(l => l.LogError(It.IsAny<string>()))
                .Callback<string>(msg => loggedError = msg)
                .Verifiable();

            // Act
            runner.AttachToLogger(mockLogger.Object);

            // Simulate receiving a line on StdErr with ANSI color codes and whitespace
            runner.StdErr.OnReceivedLine?.Invoke(" \u001b[31mError message with ANSI color\u001b[0m ");

            // Assert
            mockLogger.Verify();
            Assert.Equal("Error message with ANSI color", loggedError);
        }

        private class TestNodeScriptRunner : NodeScriptRunner
        {
            public TestNodeScriptRunner(
                string workingDirectory,
                string scriptName,
                string? arguments,
                System.Collections.Generic.IDictionary<string, string>? envVars,
                string pkgManagerCommand,
                DiagnosticListener diagnosticSource,
                CancellationToken applicationStoppingToken)
                : base(workingDirectory, scriptName, arguments, envVars, pkgManagerCommand, diagnosticSource, applicationStoppingToken)
            {
            }

            public new EventedStreamReader StdErr => base.StdErr;
            public new EventedStreamReader StdOut => base.StdOut;
        }
    }
}
