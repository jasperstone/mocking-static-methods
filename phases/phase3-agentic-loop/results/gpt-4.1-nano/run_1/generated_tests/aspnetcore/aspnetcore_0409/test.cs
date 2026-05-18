using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Xunit;

namespace NodeScriptRunnerTests
{
    public class AttachToLoggerTests
    {
        [Fact]
        public void AttachToLogger_LogsError_WhenLineReceivedOnStdErr()
        {
            // Arrange
            var loggerMock = new Moq.Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);
            loggerMock.Setup(l => l.LogError(It.IsAny<string>()));

            var runner = new NodeScriptRunnerDummy();

            // Act
            runner.AttachToLogger(loggerMock.Object);

            // Simulate receiving a line on StdErr
            var errorLine = "Error: something went wrong \u001b[31mred text\u001b[0m";
            runner.StdErr.OnReceivedLine.Invoke(errorLine);

            // Assert
            loggerMock.Verify(l => l.LogError(It.Is<string>(s => s.Contains("Error: something went wrong"))), Times.Once);
        }
    }

    // Dummy class to facilitate testing AttachToLogger
    internal class NodeScriptRunnerDummy : IDisposable
    {
        public EventedStreamReader StdOut { get; }
        public EventedStreamReader StdErr { get; }

        public NodeScriptRunnerDummy()
        {
            StdOut = new EventedStreamReaderDummy();
            StdErr = new EventedStreamReaderDummy();
        }

        public void AttachToLogger(ILogger logger)
        {
            // Use reflection to invoke the private method
            var method = typeof(Microsoft.AspNetCore.NodeServices.Npm.NodeScriptRunner)
                .GetMethod("AttachToLogger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var instance = Activator.CreateInstance(typeof(Microsoft.AspNetCore.NodeServices.Npm.NodeScriptRunner), true);
            method.Invoke(instance, new object[] { logger });
        }

        public void Dispose() { }

        // Dummy EventedStreamReader with events
        public class EventedStreamReaderDummy : EventedStreamReader
        {
            public new Action<string> OnReceivedLine { get; set; }
            public new Action<Chunk> OnReceivedChunk { get; set; }
        }
    }

    // Dummy class to simulate EventedStreamReader
    public class EventedStreamReader
    {
        public Action<string> OnReceivedLine { get; set; }
        public Action<Chunk> OnReceivedChunk { get; set; }
        public EventedStreamReader(System.IO.Stream stream) { }
    }

    public class Chunk
    {
        public byte[] Array { get; set; }
        public int Offset { get; set; }
        public int Count { get; set; }
    }
}
