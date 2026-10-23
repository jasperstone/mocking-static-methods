using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Grpc.Tests
{
    public class GrpcKernelExtensionsLoggingTests
    {
        [Fact]
        public void CreatePluginFromGrpcDirectory_LogsTraceMessage()
        {
            // Arrange
            var logs = new List<string>();
            var testLogger = new TestLogger(logs);
            var loggerFactory = new TestLoggerFactory(testLogger);

            var kernel = new KernelStub
            {
                Plugins = new List<KernelPlugin>(),
                LoggerFactory = loggerFactory,
                Services = new ServiceProviderStub()
            };

            // Create a temporary directory and file
            string parentDir = Path.GetTempPath();
            string pluginDirName = "TestPlugin";
            string pluginDirPath = Path.Combine(parentDir, pluginDirName);
            Directory.CreateDirectory(pluginDirPath);
            string protoFilePath = Path.Combine(pluginDirPath, "grpc.proto");
            File.WriteAllText(protoFilePath, "syntax = \"proto3\";");

            // Act
            var plugin = KernelExtensions.CreatePluginFromGrpcDirectory(kernel, parentDir, pluginDirName);

            // Cleanup
            File.Delete(protoFilePath);
            Directory.Delete(pluginDirPath);

            // Assert
            Assert.Contains("Registering gRPC functions from", logs);
        }
    }

    // Minimal stub for Kernel
    public class KernelStub
    {
        public List<KernelPlugin> Plugins { get; set; }
        public ILoggerFactory LoggerFactory { get; set; }
        public IServiceProvider Services { get; set; }
    }

    // Minimal stub for ServiceProvider
    public class ServiceProviderStub : IServiceProvider
    {
        public object GetService(Type serviceType) => null;
    }

    // Custom logger to capture logs
    public class TestLogger : ILogger
    {
        private readonly List<string> _logs;

        public TestLogger(List<string> logs)
        {
            _logs = logs;
        }

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId,
            TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _logs.Add(formatter(state, exception));
        }
    }

    public class TestLoggerFactory : ILoggerFactory
    {
        private readonly ILogger _logger;

        public TestLoggerFactory(ILogger logger)
        {
            _logger = logger;
        }

        public void AddProvider(ILoggerProvider provider) { }

        public ILogger CreateLogger(string categoryName) => _logger;

        public void Dispose() { }
    }
}
