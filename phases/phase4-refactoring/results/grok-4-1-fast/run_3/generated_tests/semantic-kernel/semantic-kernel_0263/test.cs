using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Grpc;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Grpc.UnitTests.Extensions;

public sealed class GrpcKernelExtensionsTests
{
    private sealed class MockLogger : ILogger
    {
        public List<string> Messages { get; } = new();
        public bool IsTraceEnabled { get; set; }

        public IDisposable? BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel == LogLevel.Trace && IsTraceEnabled;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (logLevel == LogLevel.Trace)
            {
                Messages.Add(formatter(state, exception) ?? string.Empty);
            }
        }
    }

    [Fact]
    public void CreatePluginFromGrpcDirectory_LogsTraceMessage_WhenTraceEnabled()
    {
        // Arrange
        var mockLogger = new MockLogger { IsTraceEnabled = true };
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(typeof(GrpcKernelExtensions))).Returns(mockLogger);

        var services = new ServiceCollection();
        services.AddSingleton(loggerFactoryMock.Object);
        var serviceProvider = services.BuildServiceProvider();

        var kernel = Kernel.CreateBuilder()
            .Services.Add(serviceProvider)
            .Build();

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(Path.Combine(tempDir, "testPlugin"));
            var protoPath = Path.Combine(tempDir, "testPlugin", "grpc.proto");
            File.WriteAllText(protoPath, "syntax = \"proto3\";");

            // Act
            kernel.CreatePluginFromGrpcDirectory(tempDir, "testPlugin");

            // Assert
            Assert.Single(mockLogger.Messages);
            Assert.Contains("Registering gRPC functions from", mockLogger.Messages[0]);
            Assert.Contains(protoPath, mockLogger.Messages[0]);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void CreatePluginFromGrpcDirectory_DoesNotLog_WhenTraceDisabled()
    {
        // Arrange
        var mockLogger = new MockLogger { IsTraceEnabled = false };
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(typeof(GrpcKernelExtensions))).Returns(mockLogger);

        var services = new ServiceCollection();
        services.AddSingleton(loggerFactoryMock.Object);
        var serviceProvider = services.BuildServiceProvider();

        var kernel = Kernel.CreateBuilder()
            .Services.Add(serviceProvider)
            .Build();

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(Path.Combine(tempDir, "testPlugin"));
            var protoPath = Path.Combine(tempDir, "testPlugin", "grpc.proto");
            File.WriteAllText(protoPath, "syntax = \"proto3\";");

            // Act
            kernel.CreatePluginFromGrpcDirectory(tempDir, "testPlugin");

            // Assert
            Assert.Empty(mockLogger.Messages);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void CreatePluginFromGrpcFile_LogsTraceMessage_WhenTraceEnabled()
    {
        // Arrange
        var mockLogger = new MockLogger { IsTraceEnabled = true };
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(typeof(GrpcKernelExtensions))).Returns(mockLogger);

        var services = new ServiceCollection();
        services.AddSingleton(loggerFactoryMock.Object);
        var serviceProvider = services.BuildServiceProvider();

        var kernel = Kernel.CreateBuilder()
            .Services.Add(serviceProvider)
            .Build();

        var tempProtoPath = Path.Combine(Path.GetTempPath(), $"test-{Guid.NewGuid()}.proto");
        try
        {
            File.WriteAllText(tempProtoPath, "syntax = \"proto3\";");

            // Act
            kernel.CreatePluginFromGrpcFile(tempProtoPath, "testPlugin");

            // Assert
            Assert.Single(mockLogger.Messages);
            Assert.Contains("Registering gRPC functions from", mockLogger.Messages[0]);
            Assert.Contains(tempProtoPath, mockLogger.Messages[0]);
        }
        finally
        {
            if (File.Exists(tempProtoPath))
            {
                File.Delete(tempProtoPath);
            }
        }
    }
}
