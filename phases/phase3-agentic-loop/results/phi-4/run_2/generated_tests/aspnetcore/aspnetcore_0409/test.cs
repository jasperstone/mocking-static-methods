using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.NodeServices.Npm.Tests
{
    public class NodeScriptRunnerTests
    {
        [Fact]
        public void AttachToLogger_LogsError_WhenStdErrReceivesLine()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var diagnosticSource = new Mock<DiagnosticSource>().Object;
            var applicationStoppingToken = System.Threading.CancellationToken.None;

            // Simulate the EventedStreamReader behavior
            Action<string> onReceivedLine = null;
            var nodeScriptRunner = new NodeScriptRunner(
                workingDirectory: "test",
                scriptName: "test-script",
                arguments: null,
                envVars: null,
                pkgManagerCommand: "npm",
                diagnosticSource: diagnosticSource,
                applicationStoppingToken: applicationStoppingToken)
            {
                StdErr = new Mock<EventedStreamReader>().Object
                {
                    OnReceivedLine = onReceivedLine
                }
            };

            var lineWithAnsi = "\x1b[31mError message\x1b[0m";
            var expectedLine = "Error message";

            // Act
            nodeScriptRunner.AttachToLogger(loggerMock.Object);
            onReceivedLine = line => nodeScriptRunner.StdErr.OnReceivedLine(line);
            nodeScriptRunner.StdErr.OnReceivedLine(lineWithAnsi);

            // Assert
            loggerMock.Verify(
                l => l.LogError(It.Is<string>(s => s == expectedLine)),
                Times.Once);
        }
    }
}
