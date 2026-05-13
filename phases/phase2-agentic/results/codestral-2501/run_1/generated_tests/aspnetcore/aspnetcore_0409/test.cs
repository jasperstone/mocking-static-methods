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
        var eventedStreamReaderMock = new Mock<EventedStreamReader>(MockBehavior.Strict);

        var nodeScriptRunner = new NodeScriptRunner(
            workingDirectory: "testDir",
            scriptName: "testScript",
            arguments: null,
            envVars: null,
            pkgManagerCommand: "npm",
            diagnosticSource: diagnosticSourceMock.Object,
            applicationStoppingToken: CancellationToken.None
        );

        nodeScriptRunner.StdErr = eventedStreamReaderMock.Object;

        var errorLine = "Error message with ANSI colors";
        var strippedErrorLine = "Error message with ANSI colors";

        // Act
        nodeScriptRunner.AttachToLogger(loggerMock.Object);
        eventedStreamReaderMock.Raise(e => e.OnReceivedLine += null, strippedErrorLine);

        // Assert
        loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == strippedErrorLine),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
