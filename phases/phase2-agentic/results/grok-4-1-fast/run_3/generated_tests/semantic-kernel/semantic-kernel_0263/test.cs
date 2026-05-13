using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Grpc;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Grpc.Tests.Extensions;

public class GrpcKernelExtensionsTests
{
    [Fact]
    public void CreatePluginFromGrpcDirectory_LogsTrace_WhenTraceEnabled()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var pluginDir = Path.Combine(tempDir, "MyPlugin");
        var protoPath = Path.Combine(pluginDir, "grpc.proto");
        try
        {
            Directory.CreateDirectory(pluginDir);
            File.WriteAllText(protoPath, "// dummy proto");

            var loggerMock = new Mock<ILogger<GrpcKernelExtensions>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(typeof(GrpcKernelExtensions))).Returns(loggerMock.Object);

            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(loggerFactoryMock.Object);
            var serviceProvider = services.BuildServiceProvider();

            var kernel = Kernel.CreateBuilder()
                .Services(serviceProvider)
                .Build();

            // Act
            var plugin = kernel.CreatePluginFromGrpcDirectory(tempDir, "MyPlugin");

            // Assert
            loggerMock.Verify(
                l => l.LogTrace(
                    "Registering gRPC functions from {FilePath} .proto document",
                    protoPath),
                Times.Once);
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
    public void CreatePluginFromGrpcDirectory_DoesNotLogTrace_WhenTraceDisabled()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var pluginDir = Path.Combine(tempDir, "MyPlugin");
        var protoPath = Path.Combine(pluginDir, "grpc.proto");
        try
        {
            Directory.CreateDirectory(pluginDir);
            File.WriteAllText(protoPath, "// dummy proto");

            var loggerMock = new Mock<ILogger<GrpcKernelExtensions>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(typeof(GrpcKernelExtensions))).Returns(loggerMock.Object);

            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(loggerFactoryMock.Object);
            var serviceProvider = services.BuildServiceProvider();

            var kernel = Kernel.CreateBuilder()
                .Services(serviceProvider)
                .Build();

            // Act
            var plugin = kernel.CreatePluginFromGrpcDirectory(tempDir, "MyPlugin");

            // Assert
            loggerMock.Verify(
                l => l.LogTrace(
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Never);
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
    public void CreatePluginFromGrpcFile_LogsTrace_WhenTraceEnabled()
    {
        // Arrange
        var protoPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".proto");
        try
        {
            File.WriteAllText(protoPath, "// dummy proto");

            var loggerMock = new Mock<ILogger<GrpcKernelExtensions>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(typeof(GrpcKernelExtensions))).Returns(loggerMock.Object);

            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(loggerFactoryMock.Object);
            var serviceProvider = services.BuildServiceProvider();

            var kernel = Kernel.CreateBuilder()
                .Services(serviceProvider)
                .Build();

            // Act
            var plugin = kernel.CreatePluginFromGrpcFile(protoPath, "MyPlugin");

            // Assert
            loggerMock.Verify(
                l => l.LogTrace(
                    "Registering gRPC functions from {FilePath} .proto document",
                    protoPath),
                Times.Once);
        }
        finally
        {
            if (File.Exists(protoPath))
            {
                File.Delete(protoPath);
            }
        }
    }

    [Fact]
    public void CreatePluginFromGrpcFile_DoesNotLogTrace_WhenTraceDisabled()
    {
        // Arrange
        var protoPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".proto");
        try
        {
            File.WriteAllText(protoPath, "// dummy proto");

            var loggerMock = new Mock<ILogger<GrpcKernelExtensions>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(typeof(GrpcKernelExtensions))).Returns(loggerMock.Object);

            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(loggerFactoryMock.Object);
            var serviceProvider = services.BuildServiceProvider();

            var kernel = Kernel.CreateBuilder()
                .Services(serviceProvider)
                .Build();

            // Act
            var plugin = kernel.CreatePluginFromGrpcFile(protoPath, "MyPlugin");

            // Assert
            loggerMock.Verify(
                l => l.LogTrace(
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Never);
        }
        finally
        {
            if (File.Exists(protoPath))
            {
                File.Delete(protoPath);
            }
        }
    }
}
