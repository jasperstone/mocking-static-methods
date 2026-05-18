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
        public void AttachToLogger_LogErrorCalled_WhenStdErrEmitsLine()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var nodeScriptRunner = new NodeScriptRunner(
                workingDirectory: "/path/to/working/directory",
                scriptName: "script-name",
                arguments: null,
                envVars: null,
                pkgManagerCommand: "npm",
                diagnosticSource: null,
                applicationStoppingToken: default);

            // Act
            nodeScriptRunner.AttachToLogger(loggerMock.Object);
            nodeScriptRunner.StdErr.OnReceivedLine?.Invoke("Error message");

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void AttachToLogger_LogInformationCalled_WhenStdOutEmitsLine()
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
                diagnosticSource: null,
                applicationStoppingToken: default);

            // Act
            nodeScriptRunner.AttachToLogger(loggerMock.Object);
            nodeScriptRunner.StdOut.OnReceivedLine?.Invoke("Info message");

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
        }
    }
}
