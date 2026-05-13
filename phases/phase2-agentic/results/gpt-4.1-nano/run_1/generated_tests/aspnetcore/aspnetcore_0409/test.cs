using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.NodeServices.Npm.Tests
{
    public class NodeScriptRunnerTests
    {
        [Fact]
        public void AttachToLogger_LogsErrorOnStdErrLine()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);
            mockLogger.Setup(l => l.LogError(It.IsAny<string>()));

            var dummyProcess = new Process();
            var runner = new NodeScriptRunnerDummy(dummyProcess);

            // Act
            runner.AttachToLogger(mockLogger.Object);

            // Simulate an error line
            string errorLine = "\u001b[31mError message\u001b[0m";
            runner.StdErr.OnReceivedLine.Invoke(errorLine);

            // Assert
            mockLogger.Verify(l => l.LogError(It.Is<string>(s => s == "Error message")), Times.Once);
        }
    }

    // Dummy class to access protected members for testing
    internal class NodeScriptRunnerDummy : NodeScriptRunner
    {
        public new EventedStreamReader StdOut => base.StdOut;
        public new EventedStreamReader StdErr => base.StdErr;

        public NodeScriptRunnerDummy(Process process)
        {
            // Initialize with dummy process
            _npmProcess = process;
            StdOut = new EventedStreamReader(new StringReader(""));
            StdErr = new EventedStreamReader(new StringReader(""));
        }
    }
}
