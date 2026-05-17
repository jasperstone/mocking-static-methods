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

            nodeScriptRunner.AttachToLogger(loggerMock.Object);

            var stdErr = nodeScriptRunner.StdErr;
            var stdErrStream = new MemoryStream();
            var stdErrWriter = new StreamWriter(stdErrStream);
            stdErrWriter.WriteLine("Error message");
            stdErrWriter.Flush();
            stdErrStream.Position = 0;

            var stdErrStreamReader = new EventedStreamReader(stdErrStream);
            nodeScriptRunner.StdErr = stdErrStreamReader;

            // Act
            nodeScriptRunner.StdErr.StartReading();
            await Task.Delay(100);

            // Assert
            loggerMock.Verify(logger => logger.LogError(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task AttachToLogger_LogsInformation_WhenStdOutEmitsLine()
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

            nodeScriptRunner.AttachToLogger(loggerMock.Object);

            var stdOut = nodeScriptRunner.StdOut;
            var stdOutStream = new MemoryStream();
            var stdOutWriter = new StreamWriter(stdOutStream);
            stdOutWriter.WriteLine("Info message");
            stdOutWriter.Flush();
            stdOutStream.Position = 0;

            var stdOutStreamReader = new EventedStreamReader(stdOutStream);
            nodeScriptRunner.StdOut = stdOutStreamReader;

            // Act
            nodeScriptRunner.StdOut.StartReading();
            await Task.Delay(100);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation(It.IsAny<string>()), Times.Once);
        }
    }
}
