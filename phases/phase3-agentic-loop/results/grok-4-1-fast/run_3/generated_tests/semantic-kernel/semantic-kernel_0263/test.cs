using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Grpc;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Grpc.Tests;

public class GrpcKernelExtensionsTests
{
    private const string TestProtoPath = "/test/grpc.proto";
    private const string TestPluginName = "TestPlugin";
    private const string TestParentDir = "/test/parent";
    private const string TestPluginDir = "/test/parent/TestPlugin";

    [Fact]
    public void CreatePluginFromGrpcDirectory_LogsTrace_WhenTraceEnabled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(loggerMock.Object);

        var services = new ServiceCollection();
        services.AddSingleton(loggerFactoryMock.Object);
        var serviceProvider = services.BuildServiceProvider();

        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Services.AddSingleton(serviceProvider);
        var kernel = kernelBuilder.Build();

        Directory.CreateDirectory(TestPluginDir);
        File.WriteAllText(Path.Combine(TestPluginDir, "grpc.proto"), "syntax = \"proto3\";");

        // Act
        kernel.CreatePluginFromGrpcDirectory(TestParentDir, TestPluginName);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v!.ToString()!.Contains("Registering gRPC functions from /test/parent/TestPlugin/grpc.proto")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void CreatePluginFromGrpcDirectory_DoesNotLog_WhenTraceDisabled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);

        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(loggerMock.Object);

        var services = new ServiceCollection();
        services.AddSingleton(loggerFactoryMock.Object);
        var serviceProvider = services.BuildServiceProvider();

        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Services.AddSingleton(serviceProvider);
        var kernel = kernelBuilder.Build();

        Directory.CreateDirectory(TestPluginDir);
        File.WriteAllText(Path.Combine(TestPluginDir, "grpc.proto"), "syntax = \"proto3\";");

        // Act
        kernel.CreatePluginFromGrpcDirectory(TestParentDir, TestPluginName);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public void CreatePluginFromGrpcFile_LogsTrace_WhenTraceEnabled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(loggerMock.Object);

        var services = new ServiceCollection();
        services.AddSingleton(loggerFactoryMock.Object);
        var serviceProvider = services.BuildServiceProvider();

        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Services.AddSingleton(serviceProvider);
        var kernel = kernelBuilder.Build();

        File.WriteAllText(TestProtoPath, "syntax = \"proto3\";");

        // Act
        kernel.CreatePluginFromGrpcFile(TestProtoPath, TestPluginName);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v!.ToString()!.Contains("Registering gRPC functions from /test/grpc.proto")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void CreatePluginFromGrpcFile_DoesNotLog_WhenTraceDisabled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);

        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(loggerMock.Object);

        var services = new ServiceCollection();
        services.AddSingleton(loggerFactoryMock.Object);
        var serviceProvider = services.BuildServiceProvider();

        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Services.AddSingleton(serviceProvider);
        var kernel = kernelBuilder.Build();

        File.WriteAllText(TestProtoPath, "syntax = \"proto3\";");

        // Act
        kernel.CreatePluginFromGrpcFile(TestProtoPath, TestPluginName);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
