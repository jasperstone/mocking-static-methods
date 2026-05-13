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
        var nodeScriptRunner = new NodeScriptRunner(
            workingDirectory: "testDir",
            scriptName: "testScript",
            arguments: null,
            envVars: null,
            pkgManagerCommand: "npm",
            diagnosticSource: new DiagnosticListener("test"),
            applicationStoppingToken: CancellationToken.None);

        var stdErrMock = new Mock<EventedStreamReader>();
        stdErrMock.Setup(m => m.OnReceivedLine).Raises(eventToRaise => eventToRaise += null, new object[] { "Error message" });

        nodeScriptRunner.StdErr = stdErrMock.Object;

        // Act
        nodeScriptRunner.AttachToLogger(loggerMock.Object);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error message")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
    }

    [Fact]
    public void AttachToLogger_DoesNotLogError_WhenStdErrReceivesEmptyLine()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var nodeScriptRunner = new NodeScriptRunner(
            workingDirectory: "testDir",
            scriptName: "testScript",
            arguments: null,
            envVars: null,
            pkgManagerCommand: "npm",
            diagnosticSource: new DiagnosticListener("test"),
            applicationStoppingToken: CancellationToken.None);

        var stdErrMock = new Mock<EventedStreamReader>();
        stdErrMock.Setup(m => m.OnReceivedLine).Raises(eventToRaise => eventToRaise += null, new object[] { "" });

        nodeScriptRunner.StdErr = stdErrMock.Object;

        // Act
        nodeScriptRunner.AttachToLogger(loggerMock.Object);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Never);
    }

    [Fact]
    public void AttachToLogger_LogsInformation_WhenStdOutReceivesLine()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var nodeScriptRunner = new NodeScriptRunner(
            workingDirectory: "testDir",
            scriptName: "testScript",
            arguments: null,
            envVars: null,
            pkgManagerCommand: "npm",
            diagnosticSource: new DiagnosticListener("test"),
            applicationStoppingToken: CancellationToken.None);

        var stdOutMock = new Mock<EventedStreamReader>();
        stdOutMock.Setup(m => m.OnReceivedLine).Raises(eventToRaise => eventToRaise += null, new object[] { "Information message" });

        nodeScriptRunner.StdOut = stdOutMock.Object;

        // Act
        nodeScriptRunner.AttachToLogger(loggerMock.Object);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Information message")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
    }

    [Fact]
    public void AttachToLogger_DoesNotLogInformation_WhenStdOutReceivesEmptyLine()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var nodeScriptRunner = new NodeScriptRunner(
            workingDirectory: "testDir",
            scriptName: "testScript",
            arguments: null,
            envVars: null,
            pkgManagerCommand: "npm",
            diagnosticSource: new DiagnosticListener("test"),
            applicationStoppingToken: CancellationToken.None);

        var stdOutMock = new Mock<EventedStreamReader>();
        stdOutMock.Setup(m => m.OnReceivedLine).Raises(eventToRaise => eventToRaise += null, new object[] { "" });

        nodeScriptRunner.StdOut = stdOutMock.Object;

        // Act
        nodeScriptRunner.AttachToLogger(loggerMock.Object);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Never);
    }
}
