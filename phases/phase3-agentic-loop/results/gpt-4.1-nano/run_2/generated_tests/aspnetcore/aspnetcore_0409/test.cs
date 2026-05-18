using System;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Microsoft.AspNetCore.NodeServices.Npm;

public class NodeScriptRunnerTests
{
    [Fact]
    public void AttachToLogger_LogsError_WhenStdErrLineReceived()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);
        mockLogger.Setup(l => l.LogError(It.IsAny<string>()));

        // Create a dummy process with standard error stream
        var dummyProcess = new Process();
        var runner = new NodeScriptRunnerDummy(dummyProcess);

        // Act
        runner.AttachToLogger(mockLogger.Object);

        // Simulate receiving an error line with ANSI colors
        string errorLine = "\u001b[31mError: Something went wrong\u001b[0m";
        runner.StdErr.OnReceivedLine.Invoke(errorLine);

        // Assert
        mockLogger.Verify(l => l.LogError(It.Is<string>(s => s.Contains("Error: Something went wrong"))), Times.Once);
    }
}

// Dummy class to facilitate testing
internal class NodeScriptRunnerDummy : NodeScriptRunner
{
    public new EventedStreamReader StdErr => base.StdErr;

    public NodeScriptRunnerDummy(Process process)
    {
        // Initialize with dummy process
        _npmProcess = process;
        StdOut = new EventedStreamReader(process.StandardOutput);
        StdErr = new EventedStreamReader(process.StandardError);
    }
}
