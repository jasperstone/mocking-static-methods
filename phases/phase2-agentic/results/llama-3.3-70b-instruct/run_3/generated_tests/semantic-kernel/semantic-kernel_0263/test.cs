using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Grpc.Tests
{
    public class GrpcKernelExtensionsTests
    {
        [Fact]
        public void CreatePluginFromGrpcDirectory_LogsTrace_WhenEnabled()
        {
            // Arrange
            var kernel = new Mock<Kernel>();
            kernel.Setup(k => k.LoggerFactory).Returns(new LoggerFactory());
            var loggerFactory = kernel.Object.LoggerFactory;
            var logger = loggerFactory.CreateLogger(typeof(GrpcKernelExtensions));
            loggerFactory.AddProvider(new TestLoggerProvider());
            var testLoggerProvider = (TestLoggerProvider)loggerFactory.Providers.FirstOrDefault(p => p is TestLoggerProvider);
            testLoggerProvider.Logger.IsEnabled(LogLevel.Trace, true);
            var parentDirectory = "parentDirectory";
            var pluginDirectoryName = "pluginDirectoryName";
            var protoFile = "grpc.proto";
            var filePath = Path.Combine(parentDirectory, pluginDirectoryName, protoFile);
            File.Create(filePath).Dispose();

            // Act
            GrpcKernelExtensions.CreatePluginFromGrpcDirectory(kernel.Object, parentDirectory, pluginDirectoryName);

            // Assert
            Assert.Single(testLoggerProvider.Logs);
            Assert.Contains($"Registering gRPC functions from {filePath} .proto document", testLoggerProvider.Logs[0].Message);
        }

        [Fact]
        public void CreatePluginFromGrpcFile_LogsTrace_WhenEnabled()
        {
            // Arrange
            var kernel = new Mock<Kernel>();
            kernel.Setup(k => k.LoggerFactory).Returns(new LoggerFactory());
            var loggerFactory = kernel.Object.LoggerFactory;
            var logger = loggerFactory.CreateLogger(typeof(GrpcKernelExtensions));
            loggerFactory.AddProvider(new TestLoggerProvider());
            var testLoggerProvider = (TestLoggerProvider)loggerFactory.Providers.FirstOrDefault(p => p is TestLoggerProvider);
            testLoggerProvider.Logger.IsEnabled(LogLevel.Trace, true);
            var filePath = "filePath";
            File.Create(filePath).Dispose();

            // Act
            GrpcKernelExtensions.CreatePluginFromGrpcFile(kernel.Object, filePath, "pluginName");

            // Assert
            Assert.Single(testLoggerProvider.Logs);
            Assert.Contains($"Registering gRPC functions from {filePath} .proto document", testLoggerProvider.Logs[0].Message);
        }

        private class TestLoggerProvider : ILoggerProvider
        {
            public TestLogger Logger { get; } = new TestLogger();
            public ILogger CreateLogger(string categoryName) => Logger;
            public void Dispose() { }
        }

        private class TestLogger : ILogger
        {
            public List<LogMessage> Logs { get; } = new List<LogMessage>();
            public IDisposable BeginScope<TState>(TState state) => NullDisposable.Instance;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                var message = formatter(state, exception);
                Logs.Add(new LogMessage { Message = message });
            }
        }

        private class LogMessage
        {
            public string Message { get; set; }
        }

        private class NullDisposable : IDisposable
        {
            public static readonly NullDisposable Instance = new NullDisposable();
            public void Dispose() { }
        }
    }
}
