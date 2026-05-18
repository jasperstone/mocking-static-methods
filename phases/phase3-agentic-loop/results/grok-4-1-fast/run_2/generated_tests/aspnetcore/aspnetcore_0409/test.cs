using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.NodeServices.Npm;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.NodeServices.Npm.Tests;

public class NodeScriptRunnerTests
{
    [Fact]
    public void AttachToLogger_StdErrLineReceived_LogsError()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        logger.Setup(l => l.IsEnabled(LogLevel.Error)).Returns(true);
        
        var runner = new NodeScriptRunner(
            workingDirectory: "/tmp",
            scriptName: "test",
            arguments: null,
            envVars: null,
            pkgManagerCommand: "npm",
            diagnosticSource: NullDiagnosticSource.Instance,
            applicationStoppingToken: CancellationToken.None);

        var testLine = "test error message";
        runner.AttachToLogger(logger.Object);

        // Act
        runner.StdErr.OnReceivedLine!(testLine);

        // Assert
        logger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => state.ToString()!.Contains(testLine)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void AttachToLogger_StdErrLineWithAnsi_LogsStrippedMessage()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        logger.Setup(l => l.IsEnabled(LogLevel.Error)).Returns(true);
        
        var runner = new NodeScriptRunner(
            workingDirectory: "/tmp",
            scriptName: "test",
            arguments: null,
            envVars: null,
            pkgManagerCommand: "npm",
            diagnosticSource: NullDiagnosticSource.Instance,
            applicationStoppingToken: CancellationToken.None);

        var ansiLine = "\x001b[31mtest error message\x001b[0m";
        var expectedClean = "test error message";
        runner.AttachToLogger(logger.Object);

        // Act
        runner.StdErr.OnReceivedLine!(ansiLine);

        // Assert
        logger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => state.ToString()!.Contains(expectedClean)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void AttachToLogger_StdErrEmptyLine_DoesNotLog()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        var runner = new NodeScriptRunner(
            workingDirectory: "/tmp",
            scriptName: "test",
            arguments: null,
            envVars: null,
            pkgManagerCommand: "npm",
            diagnosticSource: NullDiagnosticSource.Instance,
            applicationStoppingToken: CancellationToken.None);

        runner.AttachToLogger(logger.Object);

        // Act
        runner.StdErr.OnReceivedLine!("");
        runner.StdErr.OnReceivedLine!("   ");

        // Assert
        logger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
