using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore.NodeServices.Npm;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class NodeScriptRunnerTests
{
    [Fact]
    public void AttachToLogger_LogsError_WhenStdErrReceivesLine()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var diagnosticSourceMock = new Mock<DiagnosticSource>();
        var nodeScriptRunner = new NodeScriptRunner(
            workingDirectory: "testDir",
            scriptName: "testScript",
            arguments: null,
            envVars: null,
            pkgManagerCommand: "npm",
            diagnosticSource: diagnosticSourceMock.Object,
            applicationStoppingToken: CancellationToken.None
        );

        var stdErrMock = new Mock<EventedStreamReader>();
        stdErrMock.Setup(s => s.OnReceivedLine).Raises(eventToRaise => eventToRaise += null, EventArgs.Empty);

        nodeScriptRunner.StdErr = stdErrMock.Object;

        // Act
        nodeScriptRunner.AttachToLogger(loggerMock.Object);
        stdErrMock.Raise(s => s.OnReceivedLine += null, EventArgs.Empty);

        // Assert
        loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("test")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
