using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.AspNetCore.NodeServices.Npm;

namespace NodeScriptRunnerTests
{
    public class AttachToLoggerTests
    {
        [Fact]
        public void AttachToLogger_LogsError_WhenStdErrLineReceived()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);
            var runner = new NodeScriptRunner(
                workingDirectory: "dir",
                scriptName: "script",
                arguments: null,
                envVars: null,
                pkgManagerCommand: "npm",
                diagnosticSource: new DiagnosticListener("Test"),
                applicationStoppingToken: new System.Threading.CancellationToken());

            // Simulate StdErr.OnReceivedLine event
            var errorLine = "\u001b[31mError message\u001b[0m"; // ANSI colored error
            bool logErrorCalled = false;
            mockLogger.Setup(l => l.LogError(It.IsAny<string>()))
                .Callback<string>(msg => {
                    logErrorCalled = true;
                    Assert.Contains("Error message", msg);
                });

            // Act
            runner.AttachToLogger(mockLogger.Object);
            // Trigger the event handler manually
            runner.StdErr.OnReceivedLine.Invoke(errorLine);

            // Assert
            Assert.True(logErrorCalled);
        }
    }
}
