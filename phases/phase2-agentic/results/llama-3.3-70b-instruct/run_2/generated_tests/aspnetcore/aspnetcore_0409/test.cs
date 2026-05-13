using Microsoft.AspNetCore.NodeServices.Npm;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Text;
using Xunit;

namespace NodeScriptRunnerTests
{
    public class NodeScriptRunnerTests
    {
        [Fact]
        public void AttachToLogger_LogsError_WhenStdErrEmitsLine()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var nodeScriptRunner = new NodeScriptRunner(
                workingDirectory: "/path/to/working/directory",
                scriptName: "script-name",
                arguments: null,
                envVars: null,
                pkgManagerCommand: "npm",
                diagnosticSource: new DiagnosticListener("DiagnosticSource"),
                applicationStoppingToken: default);

            var stdErr = new EventedStreamReader(new MemoryStream());
            nodeScriptRunner.StdErr = stdErr;

            // Act
            nodeScriptRunner.AttachToLogger(loggerMock.Object);
            stdErr.OnReceivedLine?.Invoke("Error message");

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void AttachToLogger_LogsInformation_WhenStdOutEmitsLine()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var nodeScriptRunner = new NodeScriptRunner(
                workingDirectory: "/path/to/working/directory",
                scriptName: "script-name",
                arguments: null,
                envVars: null,
                pkgManagerCommand: "npm",
                diagnosticSource: new DiagnosticListener("DiagnosticSource"),
                applicationStoppingToken: default);

            var stdOut = new EventedStreamReader(new MemoryStream());
            nodeScriptRunner.StdOut = stdOut;

            loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

            // Act
            nodeScriptRunner.AttachToLogger(loggerMock.Object);
            stdOut.OnReceivedLine?.Invoke("Info message");

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
        }
    }
}
