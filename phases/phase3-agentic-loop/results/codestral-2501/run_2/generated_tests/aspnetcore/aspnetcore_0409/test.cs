using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.AspNetCore.NodeServices.Npm;
using Microsoft.AspNetCore.NodeServices.Util;
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
        var stdErrMock = new Mock<EventedStreamReader>(MockBehavior.Strict);
        var stdOutMock = new Mock<EventedStreamReader>(MockBehavior.Strict);

        var nodeScriptRunner = new NodeScriptRunner(
            workingDirectory: "testDir",
            scriptName: "testScript",
            arguments: null,
            envVars: null,
            pkgManagerCommand: "npm",
            diagnosticSource: diagnosticSourceMock.Object,
            applicationStoppingToken: CancellationToken.None
        );

        nodeScriptRunner.StdOut = stdOutMock.Object;
        nodeScriptRunner.StdErr = stdErrMock.Object;

        // Act
        nodeScriptRunner.AttachToLogger(loggerMock.Object);
        stdErrMock.Raise(m => m.OnReceivedLine += null, "test error line");

        // Assert
        loggerMock.Verify(
            m => m.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("test error line")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
