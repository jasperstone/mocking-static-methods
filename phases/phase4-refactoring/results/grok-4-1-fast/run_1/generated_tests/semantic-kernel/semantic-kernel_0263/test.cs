using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
        loggerFactoryMock.Setup(f => f.CreateLogger(typeof(GrpcKernelExtensions))).Returns(loggerMock.Object);
        
        var services = new ServiceCollection();
        services.AddSingleton(loggerFactoryMock.Object);
        var serviceProvider = services.BuildServiceProvider();
        var kernel = Kernel.CreateBuilder().Services(serviceProvider).Build();

        var parentDirectory = Path.GetTempPath();
        var pluginDirectory = Path.Combine(parentDirectory, "testPlugin");
        var protoFilePath = Path.Combine(pluginDirectory, "grpc.proto");

        try
        {
            Directory.CreateDirectory(pluginDirectory);
            File.WriteAllText(protoFilePath, "syntax = \"proto3\";");

            // Act
            kernel.CreatePluginFromGrpcDirectory(parentDirectory, "testPlugin");

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Trace,
                    0,
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Registering gRPC functions from") && v.ToString()!.Contains("grpc.proto")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
        finally
        {
            if (Directory.Exists(pluginDirectory))
            {
                Directory.Delete(pluginDirectory, true);
            }
        }
    }

    [Fact]
    public void CreatePluginFromGrpcDirectory_DoesNotLog_WhenTraceDisabled()
    {
        // Arrange
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
        loggerFactoryMock.Setup(f => f.CreateLogger(typeof(GrpcKernelExtensions))).Returns(loggerMock.Object);
        
        var services = new ServiceCollection();
        services.AddSingleton(loggerFactoryMock.Object);
        var serviceProvider = services.BuildServiceProvider();
        var kernel = Kernel.CreateBuilder().Services(serviceProvider).Build();

        var parentDirectory = Path.GetTempPath();
        var pluginDirectory = Path.Combine(parentDirectory, "testPlugin2");
        var protoFilePath = Path.Combine(pluginDirectory, "grpc.proto");

        try
        {
            Directory.CreateDirectory(pluginDirectory);
            File.WriteAllText(protoFilePath, "syntax = \"proto3\";");

            // Act
            kernel.CreatePluginFromGrpcDirectory(parentDirectory, "testPlugin2");

            // Assert
            loggerMock.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
        }
        finally
        {
            if (Directory.Exists(pluginDirectory))
            {
                Directory.Delete(pluginDirectory, true);
            }
        }
    }

    [Fact]
    public void CreatePluginFromGrpcFile_LogsTrace_WhenTraceEnabled()
    {
        // Arrange
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
        loggerFactoryMock.Setup(f => f.CreateLogger(typeof(GrpcKernelExtensions))).Returns(loggerMock.Object);
        
        var services = new ServiceCollection();
        services.AddSingleton(loggerFactoryMock.Object);
        var serviceProvider = services.BuildServiceProvider();
        var kernel = Kernel.CreateBuilder().Services(serviceProvider).Build();

        var filePath = Path.Combine(Path.GetTempPath(), "test.proto");
        try
        {
            File.WriteAllText(filePath, "syntax = \"proto3\";");

            // Act
            kernel.CreatePluginFromGrpcFile(filePath, "testPlugin");

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Trace,
                    0,
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Registering gRPC functions from") && v.ToString()!.Contains("test.proto")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

    [Fact]
    public void CreatePluginFromGrpcFile_DoesNotLog_WhenTraceDisabled()
    {
        // Arrange
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
        loggerFactoryMock.Setup(f => f.CreateLogger(typeof(GrpcKernelExtensions))).Returns(loggerMock.Object);
        
        var services = new ServiceCollection();
        services.AddSingleton(loggerFactoryMock.Object);
        var serviceProvider = services.BuildServiceProvider();
        var kernel = Kernel.CreateBuilder().Services(serviceProvider).Build();

        var filePath = Path.Combine(Path.GetTempPath(), "test2.proto");
        try
        {
            File.WriteAllText(filePath, "syntax = \"proto3\";");

            // Act
            kernel.CreatePluginFromGrpcFile(filePath, "testPlugin");

            // Assert
            loggerMock.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

    [Fact]
    public void CreatePluginFromGrpcDirectory_ThrowsFileNotFoundException_WhenProtoFileMissing()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        var serviceProvider = services.BuildServiceProvider();
        var kernel = Kernel.CreateBuilder().Services(serviceProvider).Build();

        var parentDirectory = Path.GetTempPath();
        var pluginDirectory = Path.Combine(parentDirectory, "testPluginNoProto");

        Directory.CreateDirectory(pluginDirectory);

        // Act & Assert
        var exception = Assert.Throws<FileNotFoundException>(() => kernel.CreatePluginFromGrpcDirectory(parentDirectory, "testPluginNoProto"));
        Assert.Contains("grpc.proto", exception.Message);
    }

    [Fact]
    public void CreatePluginFromGrpcFile_ThrowsFileNotFoundException_WhenFileMissing()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        var serviceProvider = services.BuildServiceProvider();
        var kernel = Kernel.CreateBuilder().Services(serviceProvider).Build();

        var nonExistentPath = Path.Combine(Path.GetTempPath(), "nonexistent.proto");

        // Act & Assert
        var exception = Assert.Throws<FileNotFoundException>(() => kernel.CreatePluginFromGrpcFile(nonExistentPath, "testPlugin"));
        Assert.Contains(nonExistentPath, exception.Message);
    }
}
