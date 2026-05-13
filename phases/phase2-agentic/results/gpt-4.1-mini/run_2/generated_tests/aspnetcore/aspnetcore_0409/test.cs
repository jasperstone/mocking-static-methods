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
            runner.StdErr.InvokeOnReceivedLine("Error \x001b[31mmessage\x001b[0m");

            // Assert
            Assert.Single(logger.ErrorMessages);
            Assert.Equal("Error message", logger.ErrorMessages[0]);
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

            // Assert
            Assert.Empty(logger.ErrorMessages);
        }

        private static NodeScriptRunner CreateNodeScriptRunnerForTest()
        {
            // We create a NodeScriptRunner with dummy parameters and override the StdOut and StdErr
            // with testable EventedStreamReader mocks.
            var dummyWorkingDir = Directory.GetCurrentDirectory();
            var dummyScriptName = "testscript";
            var dummyArgs = "";
            var dummyEnvVars = null as System.Collections.Generic.IDictionary<string, string>;
            var dummyPkgManagerCommand = "npm";
            var dummyDiagnosticSource = new System.Diagnostics.DiagnosticListener("dummy");
            var dummyCancellationToken = new System.Threading.CancellationToken();

            var runner = new NodeScriptRunner(
                dummyWorkingDir,
                dummyScriptName,
                dummyArgs,
                dummyEnvVars,
                dummyPkgManagerCommand,
                dummyDiagnosticSource,
                dummyCancellationToken);

            // Replace StdOut and StdErr with test doubles that allow raising events
            runner.StdOut = new TestEventedStreamReader();
            runner.StdErr = new TestEventedStreamReader();

            return runner;
        }

        private class TestLogger : ILogger
        {
            public readonly System.Collections.Generic.List<string> ErrorMessages = new();

            public IDisposable BeginScope<TState>(TState state) => null!;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (logLevel == LogLevel.Error)
                {
                    ErrorMessages.Add(formatter(state, exception));
                }
            }
        }

        private class TestEventedStreamReader : EventedStreamReader
        {
            public TestEventedStreamReader() : base(new MemoryStreamReader()) { }

            public void InvokeOnReceivedLine(string line)
            {
                OnReceivedLine?.Invoke(line);
            }

            private class MemoryStreamReader : System.IO.StreamReader
            {
                public MemoryStreamReader() : base(new MemoryStream()) { }
            }
        }
    }
}
