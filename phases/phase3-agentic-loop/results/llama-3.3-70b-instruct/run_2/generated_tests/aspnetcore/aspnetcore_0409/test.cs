using Microsoft.AspNetCore.NodeServices.Npm;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NodeScriptRunnerTests
{
    public class NodeScriptRunnerTests
    {
        [Fact]
        public async Task AttachToLogger_LogsError_WhenStdErrEmitsLine()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var nodeScriptRunner = new NodeScriptRunner(
                workingDirectory: "/path/to/working/directory",
                scriptName: "script-name",
                arguments: null,
                envVars: null,
                pkgManagerCommand: "npm",
                diagnosticSource: new DiagnosticSource(),
                applicationStoppingToken: default);

            var stdErr = nodeScriptRunner.StdErr;
            nodeScriptRunner.AttachToLogger(loggerMock.Object);

            // Act
            stdErr.OnReceivedLine?.Invoke("Error message");

            // Assert
            loggerMock.Verify(logger => logger.LogError(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task AttachToLogger_LogsInformation_WhenStdOutEmitsLine()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);
            var nodeScriptRunner = new NodeScriptRunner(
                workingDirectory: "/path/to/working/directory",
                scriptName: "script-name",
                arguments: null,
                envVars: null,
                pkgManagerCommand: "npm",
                diagnosticSource: new DiagnosticSource(),
                applicationStoppingToken: default);

            var stdOut = nodeScriptRunner.StdOut;
            nodeScriptRunner.AttachToLogger(loggerMock.Object);

            // Act
            stdOut.OnReceivedLine?.Invoke("Info message");

            // Assert
            loggerMock.Verify(logger => logger.LogInformation(It.IsAny<string>()), Times.Once);
        }
    }
}
