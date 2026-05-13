using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.NodeServices.Npm;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.AspNetCore.NodeServices.Npm.Tests
{
    public class NodeScriptRunnerTests
    {
        [Fact]
        public void AttachToLogger_LogsErrorOnStdErrReceivedLine()
        {
            // Arrange
            var runner = CreateNodeScriptRunnerForTest();
            var logger = new TestLogger();

            runner.AttachToLogger(logger);

            // Act
            // Simulate receiving a line on StdErr that is not null or whitespace
            runner.StdErr.InvokeOnReceivedLine("Error line with \x001b[31mred\x001b[0m color");

            // Assert
            Assert.Single(logger.ErrorMessages);
            Assert.Equal("Error line with red color", logger.ErrorMessages[0]);
        }

        [Fact]
        public void AttachToLogger_DoesNotLogErrorOnEmptyOrWhitespaceStdErrLine()
        {
            // Arrange
            var runner = CreateNodeScriptRunnerForTest();
            var logger = new TestLogger();

            runner.AttachToLogger(logger);

            // Act
            runner.StdErr.InvokeOnReceivedLine("");
            runner.StdErr.InvokeOnReceivedLine("   ");
            runner.StdErr.InvokeOnReceivedLine(null);

            // Assert
            Assert.Empty(logger.ErrorMessages);
        }

        // Helper method to create a NodeScriptRunner with mocked EventedStreamReader
        private static NodeScriptRunner CreateNodeScriptRunnerForTest()
        {
            // We need to create a NodeScriptRunner instance but the constructor requires many parameters
            // and starts a process. Instead, we create a derived class that allows us to set StdOut and StdErr manually.

            return new TestNodeScriptRunner();
        }

        // A derived class to allow setting StdOut and StdErr manually for testing
        private class TestNodeScriptRunner : NodeScriptRunner
        {
            public new EventedStreamReader StdOut { get; }
            public new EventedStreamReader StdErr { get; }

            public TestNodeScriptRunner() : base(
                workingDirectory: Directory.GetCurrentDirectory(),
                scriptName: "testscript",
                arguments: null,
                envVars: null,
                pkgManagerCommand: "npm",
                diagnosticSource: new System.Diagnostics.DiagnosticListener("Test"),
                applicationStoppingToken: default)
            {
                // Replace StdOut and StdErr with test doubles
                StdOut = new EventedStreamReader();
                StdErr = new EventedStreamReader();

                // Override the base properties with our test doubles
                base.StdOut.OnReceivedLine = StdOut.OnReceivedLine;
                base.StdErr.OnReceivedLine = StdErr.OnReceivedLine;
                base.StdErr.OnReceivedChunk = StdErr.OnReceivedChunk;
            }
        }

        // A simple test logger to capture log messages
        private class TestLogger : ILogger
        {
            public readonly System.Collections.Generic.List<string> ErrorMessages = new();
            public readonly System.Collections.Generic.List<string> InfoMessages = new();

            public IDisposable BeginScope<TState>(TState state) => null!;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                var message = formatter(state, exception);
                if (logLevel == LogLevel.Error)
                {
                    ErrorMessages.Add(message);
                }
                else if (logLevel == LogLevel.Information)
                {
                    InfoMessages.Add(message);
                }
            }
        }

        // Minimal implementation of EventedStreamReader for testing
        private class EventedStreamReader
        {
            public delegate void ReceivedLineHandler(string? line);
            public event ReceivedLineHandler? OnReceivedLine;

            public delegate void ReceivedChunkHandler(ArraySegment<char> chunk);
            public event ReceivedChunkHandler? OnReceivedChunk;

            public EventedStreamReader()
            {
            }

            public void InvokeOnReceivedLine(string? line)
            {
                OnReceivedLine?.Invoke(line);
            }

            public void InvokeOnReceivedChunk(ArraySegment<char> chunk)
            {
                OnReceivedChunk?.Invoke(chunk);
            }
        }
    }
}
