using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.NodeServices.Npm;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;

namespace Microsoft.AspNetCore.NodeServices.Npm.Tests
{
    public class NodeScriptRunnerTests
    {
        [Fact]
        public void AttachToLogger_LogsErrorOnStdErrReceivedLine()
        {
            // Arrange
            var workingDirectory = Environment.CurrentDirectory;
            var scriptName = "testscript";
            var arguments = "arg1 arg2";
            var envVars = new Dictionary<string, string>();
            var pkgManagerCommand = "npm";
            var diagnosticSource = new DiagnosticListener("TestListener");
            var cts = new System.Threading.CancellationTokenSource();

            using var runner = new NodeScriptRunner(workingDirectory, scriptName, arguments, envVars, pkgManagerCommand, diagnosticSource, cts.Token);

            var loggerMock = new Mock<ILogger>();

            // Attach the logger to the runner
            runner.AttachToLogger(loggerMock.Object);

            // Act
            // Simulate StdErr receiving a line with ANSI colors and whitespace
            var testLine = "\x001b[31mError message with ANSI\x001b[0m";
            runner.StdErr.RaiseOnReceivedLine(testLine);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Error message with ANSI"),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Helper extensions to raise events on EventedStreamReader
    internal static class EventedStreamReaderExtensions
    {
        public static void RaiseOnReceivedLine(this EventedStreamReader reader, string line)
        {
            var eventField = typeof(EventedStreamReader).GetField("OnReceivedLine", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            var handler = (Action<string>)eventField?.GetValue(reader);
            handler?.Invoke(line);
        }
    }
}
