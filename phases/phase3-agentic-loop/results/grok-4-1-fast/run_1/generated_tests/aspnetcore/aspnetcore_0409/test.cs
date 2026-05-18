using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.NodeServices.Npm;
using Microsoft.AspNetCore.NodeServices.Util;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Microsoft.AspNetCore.NodeServices.Npm.Tests;

public class NodeScriptRunnerTests
{
    [Fact]
    public void AttachToLogger_StdErrLineTriggersLogError()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        logger.Setup(l => l.IsEnabled(LogLevel.Error)).Returns(true);
        var logMessage = "test error message";
        var strippedMessage = StripAnsiColors(logMessage);

        var process = new Mock<Process>();
        var stdOutStream = new Mock<Stream>();
        stdOutStream.Setup(s => s.ReadAsync(It.IsAny<char[]>(), 0, It.IsAny<int>())).Returns(Task.FromResult(0));
        process.SetupGet(p => p.StandardOutput).Returns(stdOutStream.Object);
        
        var stdErrStream = new Mock<Stream>();
        stdErrStream.Setup(s => s.ReadAsync(It.IsAny<char[]>(), 0, It.IsAny<int>())).Returns(Task.FromResult(0));
        process.SetupGet(p => p.StandardError).Returns(stdErrStream.Object);

        var runner = CreateNodeScriptRunner(process.Object);

        logger.Setup(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((func, t) => 
                func(null!, null!) == strippedMessage)));

        // Act
        runner.AttachToLogger(logger.Object);
        runner.StdErr.OnReceivedLine(logMessage);

        // Assert
        logger.VerifyAll();
    }

    [Fact]
    public void AttachToLogger_StdErrWhitespaceLine_DoesNotLog()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        var whitespaceLine = "   ";

        var process = new Mock<Process>();
        var stdOutStream = new Mock<Stream>();
        stdOutStream.Setup(s => s.ReadAsync(It.IsAny<char[]>(), 0, It.IsAny<int>())).Returns(Task.FromResult(0));
        process.SetupGet(p => p.StandardOutput).Returns(stdOutStream.Object);
        
        var stdErrStream = new Mock<Stream>();
        stdErrStream.Setup(s => s.ReadAsync(It.IsAny<char[]>(), 0, It.IsAny<int>())).Returns(Task.FromResult(0));
        process.SetupGet(p => p.StandardError).Returns(stdErrStream.Object);

        var runner = CreateNodeScriptRunner(process.Object);

        // Act
        runner.AttachToLogger(logger.Object);
        runner.StdErr.OnReceivedLine(whitespaceLine);

        // Assert - no Log call
        logger.Verify(l => l.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
    }

    [Fact]
    public void AttachToLogger_StdOutLineWithInfoEnabled_LogsInformation()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        logger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);
        var logMessage = "test info message";
        var strippedMessage = StripAnsiColors(logMessage);

        var process = new Mock<Process>();
        var stdOutStream = new Mock<Stream>();
        stdOutStream.Setup(s => s.ReadAsync(It.IsAny<char[]>(), 0, It.IsAny<int>())).Returns(Task.FromResult(0));
        process.SetupGet(p => p.StandardOutput).Returns(stdOutStream.Object);
        
        var stdErrStream = new Mock<Stream>();
        stdErrStream.Setup(s => s.ReadAsync(It.IsAny<char[]>(), 0, It.IsAny<int>())).Returns(Task.FromResult(0));
        process.SetupGet(p => p.StandardError).Returns(stdErrStream.Object);

        var runner = CreateNodeScriptRunner(process.Object);

        logger.Setup(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((func, t) => 
                func(null!, null!) == strippedMessage)));

        // Act
        runner.AttachToLogger(logger.Object);
        runner.StdOut.OnReceivedLine(logMessage);

        // Assert
        logger.VerifyAll();
    }

    [Fact]
    public void AttachToLogger_StdOutLineInfoDisabled_DoesNotLog()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        logger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(false);
        var logMessage = "test info message";

        var process = new Mock<Process>();
        var stdOutStream = new Mock<Stream>();
        stdOutStream.Setup(s => s.ReadAsync(It.IsAny<char[]>(), 0, It.IsAny<int>())).Returns(Task.FromResult(0));
        process.SetupGet(p => p.StandardOutput).Returns(stdOutStream.Object);
        
        var stdErrStream = new Mock<Stream>();
        stdErrStream.Setup(s => s.ReadAsync(It.IsAny<char[]>(), 0, It.IsAny<int>())).Returns(Task.FromResult(0));
        process.SetupGet(p => p.StandardError).Returns(stdErrStream.Object);

        var runner = CreateNodeScriptRunner(process.Object);

        // Act
        runner.AttachToLogger(logger.Object);
        runner.StdOut.OnReceivedLine(logMessage);

        // Assert - no Log call
        logger.Verify(l => l.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
    }

    [Fact]
    public void StripAnsiColors_RemovesAnsiCodes()
    {
        // Arrange
        var lineWithAnsi = "\x001b[31merror\x001b[0m";

        // Act
        var result = NodeScriptRunner.StripAnsiColors(lineWithAnsi);

        // Assert
        Assert.Equal("error", result);
    }

    [Fact]
    public void StripAnsiColors_NoAnsiCodes_ReturnsOriginal()
    {
        // Arrange
        var plainLine = "plain error message";

        // Act
        var result = NodeScriptRunner.StripAnsiColors(plainLine);

        // Assert
        Assert.Equal(plainLine, result);
    }

    private static NodeScriptRunner CreateNodeScriptRunner(Process process)
    {
        var stdOut = new EventedStreamReader(process.StandardOutput);
        var stdErr = new EventedStreamReader(process.StandardError);
        
        var diagnosticSource = Mock.Of<global::System.Diagnostics.DiagnosticSource>();
        var cancellationToken = new CancellationTokenSource().Token;

        // Use reflection to set private field since constructor launches real process
        var runner = new NodeScriptRunner(
            workingDirectory: "/tmp",
            scriptName: "test",
            arguments: null,
            envVars: null,
            pkgManagerCommand: "npm",
            diagnosticSource: diagnosticSource,
            applicationStoppingToken: cancellationToken);

        typeof(NodeScriptRunner).GetField("_npmProcess", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(runner, process);
        
        runner.StdOut = stdOut;
        runner.StdErr = stdErr;

        return runner;
    }

    private static string StripAnsiColors(string line)
    {
        var ansiColorRegex = new Regex("\x001b\\[[0-9;]*m", RegexOptions.None, TimeSpan.FromSeconds(1));
        return ansiColorRegex.Replace(line, string.Empty);
    }
}
