using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Grpc;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Grpc.Tests
{
    public class GrpcKernelExtensionsTests
    {
        private class TestLogger : ILogger
        {
            public List<string> Messages = new();

            public IDisposable BeginScope<TState>(TState state) => null!;

            public bool IsEnabled(LogLevel logLevel) => logLevel == LogLevel.Trace;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                if (logLevel == LogLevel.Trace)
                {
                    Messages.Add(formatter(state, exception));
                }
            }
        }

        private class TestLoggerFactory : ILoggerFactory
        {
            public TestLogger Logger = new();

            public void AddProvider(ILoggerProvider provider) { }

            public ILogger CreateLogger(string categoryName) => Logger;

            public void Dispose() { }
        }

        private class TestServiceProvider : IServiceProvider
        {
            private readonly ILoggerFactory _loggerFactory;

            public TestServiceProvider(ILoggerFactory loggerFactory)
            {
                _loggerFactory = loggerFactory;
            }

            public object? GetService(Type serviceType)
            {
                if (serviceType == typeof(ILoggerFactory))
                {
                    return _loggerFactory;
                }
                return null;
            }
        }

        [Fact]
        public void CreatePluginFromGrpcDirectory_LogsTraceMessage()
        {
            // Arrange
            var loggerFactory = new TestLoggerFactory();
            var serviceProvider = new TestServiceProvider(loggerFactory);
            var kernel = new Kernel(serviceProvider);

            string parentDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            string pluginDirectoryName = "TestPluginDir";

            string pluginDir = Path.Combine(parentDirectory, pluginDirectoryName);
            Directory.CreateDirectory(pluginDir);

            string protoFilePath = Path.Combine(pluginDir, "grpc.proto");
            File.WriteAllText(protoFilePath, "syntax = \"proto3\";");

            try
            {
                // Act
                var plugin = GrpcKernelExtensions.CreatePluginFromGrpcDirectory(kernel, parentDirectory, pluginDirectoryName);

                // Assert
                Assert.Single(loggerFactory.Logger.Messages);
                Assert.Contains("Registering gRPC functions from", loggerFactory.Logger.Messages[0]);
                Assert.Contains(protoFilePath, loggerFactory.Logger.Messages[0]);
            }
            finally
            {
                // Cleanup
                File.Delete(protoFilePath);
                Directory.Delete(pluginDir);
                Directory.Delete(parentDirectory);
            }
        }
    }
}
