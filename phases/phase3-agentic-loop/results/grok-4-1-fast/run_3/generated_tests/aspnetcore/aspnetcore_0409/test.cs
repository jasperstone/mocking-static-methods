using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.NodeServices.Npm.Tests
{
    public class NodeScriptRunnerTests
    {
        private static readonly Regex AnsiColorRegex = new Regex(@"\x001b\[[0-9;]*m", RegexOptions.None, TimeSpan.FromSeconds(1));

        [Fact]
        public void AttachToLogger_StdErrLineTriggersLogError()
        {
            // Arrange
            var logger = new Mock<ILogger>();
            logger.Setup(l => l.IsEnabled(LogLevel.Error)).Returns(true);
            var runner = CreateTestableRunner();

            // Act
            runner.AttachToLogger(logger.Object);
            runner.StdErr.OnReceivedLine?.Invoke(runner.StdErr, new LineReceivedEventArgs("error message"));

            // Assert - LogError extension calls Log with LogLevel.Error
            logger.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).ToString().Contains("error message")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void AttachToLogger_StdErrWhitespaceLine_DoesNotLog()
        {
            // Arrange
            var logger = new Mock<ILogger>();
            var runner = CreateTestableRunner();

            // Act
            runner.AttachToLogger(logger.Object);
            runner.StdErr.OnReceivedLine?.Invoke(runner.StdErr, new LineReceivedEventArgs("   \t  "));

            // Assert
            logger.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), 
                It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
        }

        [Fact]
        public void AttachToLogger_StdErrEmptyLine_DoesNotLog()
        {
            // Arrange
            var logger = new Mock<ILogger>();
            var runner = CreateTestableRunner();

            // Act
            runner.AttachToLogger(logger.Object);
            runner.StdErr.OnReceivedLine?.Invoke(runner.StdErr, new LineReceivedEventArgs(""));

            // Assert
            logger.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), 
                It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, EventId?, string>>()), Times.Never);
        }

        [Fact]
        public void AttachToLogger_StdErrStripsAnsiColors()
        {
            // Arrange
            var logger = new Mock<ILogger>();
            var runner = CreateTestableRunner();

            // Act
            runner.AttachToLogger(logger.Object);
            var ansiLine = "error\x1b[31mwith\x1b[0mcolors";
            runner.StdErr.OnReceivedLine?.Invoke(runner.StdErr, new LineReceivedEventArgs(ansiLine));

            // Assert - ANSI codes should be stripped
            var expectedCleaned = AnsiColorRegex.Replace(ansiLine, string.Empty);
            logger.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).ToString() == expectedCleaned),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private static NodeScriptRunner CreateTestableRunner()
        {
            // Create mock EventedStreamReader instances
            var mockStdOut = new Mock<EventedStreamReader>(MockBehavior.Strict, new Mock<Stream>().Object);
            var mockStdErr = new Mock<EventedStreamReader>(MockBehavior.Strict, new Mock<Stream>().Object);
            
            mockStdOut.SetupProperty(esr => esr.OnReceivedLine);
            mockStdErr.SetupProperty(esr => esr.OnReceivedLine);
            mockStdErr.SetupProperty(esr => esr.OnReceivedChunk);

            // Use reflection to create NodeScriptRunner with our mock streams
            var constructorArgs = new object[] { ".", "test", null, null, "npm", new NullDiagnosticSource(), CancellationToken.None };
            
            var runner = (NodeScriptRunner)Activator.CreateInstance(
                typeof(NodeScriptRunner), 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, 
                null, constructorArgs, null)!;

            // Use reflection to set the public properties to our mocks
            typeof(NodeScriptRunner).GetProperty("StdOut")!.SetValue(runner, mockStdOut.Object);
            typeof(NodeScriptRunner).GetProperty("StdErr")!.SetValue(runner, mockStdErr.Object);

            return runner;
        }
    }

    // Minimal DiagnosticSource for constructor
    internal class NullDiagnosticSource : DiagnosticSource
    {
        public static readonly NullDiagnosticSource Instance = new();
        public override bool IsEnabled(string name) => false;
        public override void Write(string name, object? value) { }
    }

    // Minimal EventedStreamReader for event signature matching
    internal class EventedStreamReader
    {
        public event EventHandler<LineReceivedEventArgs>? OnReceivedLine;
        public event EventHandler<ArraySegment<byte>>? OnReceivedChunk;

        public EventedStreamReader(Stream stream) { }
    }

    internal class LineReceivedEventArgs : EventArgs
    {
        public string Line { get; }
        public LineReceivedEventArgs(string line) => Line = line;
    }
}
