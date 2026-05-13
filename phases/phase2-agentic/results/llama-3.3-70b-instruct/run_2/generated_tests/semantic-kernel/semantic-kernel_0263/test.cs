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
        public void CreatePluginFromGrpcDirectory_TraceLoggingEnabled_LogsTraceMessage()
        {
            // Arrange
            var kernel = new Mock<Kernel>();
            kernel.Setup(k => k.LoggerFactory).Returns(new LoggerFactory());
            var parentDirectory = "parentDirectory";
            var pluginDirectoryName = "pluginDirectoryName";
            var filePath = Path.Combine(parentDirectory, pluginDirectoryName, "grpc.proto");
            File.Create(filePath).Dispose();

            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new MockLoggerProvider());
            kernel.Setup(k => k.LoggerFactory).Returns(loggerFactory);

            // Act
            GrpcKernelExtensions.CreatePluginFromGrpcDirectory(kernel.Object, parentDirectory, pluginDirectoryName);

            // Assert
            var loggerProvider = (MockLoggerProvider)loggerFactory.GetProvider("Mock");
            Assert.Single(loggerProvider.Logs);
            var log = loggerProvider.Logs[0];
            Assert.Equal(LogLevel.Trace, log.LogLevel);
            Assert.Contains($"Registering gRPC functions from {filePath} .proto document", log.Message);
        }

        [Fact]
        public void CreatePluginFromGrpcFile_TraceLoggingEnabled_LogsTraceMessage()
        {
            // Arrange
            var kernel = new Mock<Kernel>();
            kernel.Setup(k => k.LoggerFactory).Returns(new LoggerFactory());
            var filePath = "grpc.proto";
            File.Create(filePath).Dispose();

            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new MockLoggerProvider());
            kernel.Setup(k => k.LoggerFactory).Returns(loggerFactory);

            // Act
            GrpcKernelExtensions.CreatePluginFromGrpcFile(kernel.Object, filePath, "pluginName");

            // Assert
            var loggerProvider = (MockLoggerProvider)loggerFactory.GetProvider("Mock");
            Assert.Single(loggerProvider.Logs);
            var log = loggerProvider.Logs[0];
            Assert.Equal(LogLevel.Trace, log.LogLevel);
            Assert.Contains($"Registering gRPC functions from {filePath} .proto document", log.Message);
        }

        private class MockLoggerProvider : ILoggerProvider
        {
            public List<Log> Logs { get; } = new List<Log>();

            public ILogger CreateLogger(string categoryName)
            {
                return new MockLogger(this);
            }

            public void Dispose()
            {
            }
        }

        private class MockLogger : ILogger
        {
            private readonly MockLoggerProvider _provider;

            public MockLogger(MockLoggerProvider provider)
            {
                _provider = provider;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return NullDisposable.Instance;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                _provider.Logs.Add(new Log { LogLevel = logLevel, Message = formatter(state, exception) });
            }
        }

        private class Log
        {
            public LogLevel LogLevel { get; set; }
            public string Message { get; set; }
        }
    }
}
