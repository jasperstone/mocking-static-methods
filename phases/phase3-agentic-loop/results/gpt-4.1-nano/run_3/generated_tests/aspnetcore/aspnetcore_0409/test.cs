using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.AspNetCore.NodeServices.Npm;

namespace NodeScriptRunnerTests
{
    public class AttachToLoggerTests
    {
        [Fact]
        public void LogsError_WhenStdErrLineReceived_ShouldCallLogError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);
            var runner = new NodeScriptRunnerStub();

            // Act
            runner.AttachToLogger(mockLogger.Object);
            var errorLine = "Error: something went wrong";
            runner.InvokeStdErrLine(errorLine);

            // Assert
            mockLogger.Verify(x => x.LogError(It.Is<string>(s => s.Contains("something went wrong"))), Times.Once);
        }
    }

    // A minimal stub to simulate the NodeScriptRunner's event subscription
    internal class NodeScriptRunnerStub : NodeScriptRunner
    {
        public Action<string> OnReceivedLineStdOut;
        public Action<string> OnReceivedLineStdErr;

        public NodeScriptRunnerStub() : base("workingDir", "script", null, null, "pkg", new DiagnosticListener("test"), default)
        {
        }

        public void InvokeStdErrLine(string line)
        {
            OnReceivedLine?.Invoke(line);
        }

        protected override void AttachToLogger(ILogger logger)
        {
            // override to capture the event subscription
            this.StdErr.OnReceivedLine += line => OnReceivedLine?.Invoke(line);
        }
    }
}
