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
        public void AttachToLogger_LogsError_WhenStdErrReceivesLine()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);
            mockLogger.Setup(l => l.LogError(It.IsAny<string>()));

            var runner = new NodeScriptRunner(
                workingDirectory: "dir",
                scriptName: "script",
                arguments: null,
                envVars: null,
                pkgManagerCommand: "npm",
                diagnosticSource: new DiagnosticListener("Test"),
                applicationStoppingToken: new System.Threading.CancellationToken());

            // Simulate the process
            var stdErr = runner.GetType().GetProperty("StdErr", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(runner);
            var onReceivedLineEvent = stdErr.GetType().GetEvent("OnReceivedLine");
            var addMethod = onReceivedLineEvent.GetAddMethod();

            // Act
            // Attach the logger
            runner.AttachToLogger(mockLogger.Object);

            // Trigger the event
            var line = "Error line";
            var eventDelegate = (MulticastDelegate)onReceivedLineEvent.GetValue(stdErr);
            var invocationList = eventDelegate.GetInvocationList();

            foreach (var del in invocationList)
            {
                del.DynamicInvoke(runner, new Action<string>(l => { }) { Target = line });
            }

            // Assert
            mockLogger.Verify(l => l.LogError(It.Is<string>(s => s.Contains("Error line"))), Times.Once);
        }
    }
}
