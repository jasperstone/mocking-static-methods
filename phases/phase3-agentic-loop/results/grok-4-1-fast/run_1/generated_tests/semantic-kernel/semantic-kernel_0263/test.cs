using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Grpc;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Grpc.UnitTests.Extensions;

public class GrpcKernelExtensionsTests
{
    private const string TestProtoPath = "test.proto";
    private const string TestPluginName = "TestPlugin";

    [Fact]
    public void CreatePluginFromGrpcDirectory_LogsTrace_WhenTraceEnabled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Services.AddSingleton<ILoggerFactory>(loggerFactoryMock.Object);
        var kernel = kernelBuilder.Build();

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(Path.Combine(tempDir, TestPluginName));
            var protoPath = Path.Combine(tempDir, TestPluginName, "grpc.proto");
            File.WriteAllText(protoPath, "syntax = \"proto3\";");

            // Act
            var plugin = kernel.CreatePluginFromGrpcDirectory(tempDir, TestPluginName);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Registering gRPC functions from") && v.ToString()!.Contains("grpc.proto")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
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
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);

        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Services.AddSingleton<ILoggerFactory>(loggerFactoryMock.Object);
        var kernel = kernelBuilder.Build();

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(Path.Combine(tempDir, TestPluginName));
            var protoPath = Path.Combine(tempDir, TestPluginName, "grpc.proto");
            File.WriteAllText(protoPath, "syntax = \"proto3\";");

            // Act
            _ = kernel.CreatePluginFromGrpcDirectory(tempDir, TestPluginName);

            // Assert
            loggerMock.Verify(l => l.Log(LogLevel.Trace, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
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
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Services.AddSingleton<ILoggerFactory>(loggerFactoryMock.Object);
        var kernel = kernelBuilder.Build();

        var tempProtoPath = Path.Combine(Path.GetTempPath(), TestProtoPath);
        try
        {
            File.WriteAllText(tempProtoPath, "syntax = \"proto3\";");

            // Act
            var plugin = kernel.CreatePluginFromGrpcFile(tempProtoPath, TestPluginName);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Registering gRPC functions from") && v.ToString()!.Contains("test.proto")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
        finally
        {
            if (File.Exists(tempProtoPath))
            {
                File.Delete(tempProtoPath);
            }
        }
    }

    [Fact]
    public void CreatePluginFromGrpcFile_DoesNotLogTrace_WhenTraceDisabled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);

        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Services.AddSingleton<ILoggerFactory>(loggerFactoryMock.Object);
        var kernel = kernelBuilder.Build();

        var tempProtoPath = Path.Combine(Path.GetTempPath(), TestProtoPath);
        try
        {
            File.WriteAllText(tempProtoPath, "syntax = \"proto3\";");

            // Act
            _ = kernel.CreatePluginFromGrpcFile(tempProtoPath, TestPluginName);

            // Assert
            loggerMock.Verify(l => l.Log(LogLevel.Trace, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
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
