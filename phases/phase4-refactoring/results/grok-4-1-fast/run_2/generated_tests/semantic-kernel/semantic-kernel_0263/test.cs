using System;
using System.IO;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Grpc.Tests;

public class GrpcKernelExtensionsTests
{
    private const string TestParentDir = "/test/parent";
    private const string TestPluginDirName = "TestPlugin";
    private const string TestProtoPath = "/test/parent/TestPlugin/grpc.proto";
    private const string TestFilePath = "/test.proto";
    private const string TestPluginName = "TestPlugin";

    [Fact]
    public void CreatePluginFromGrpcDirectory_LogsTrace_WhenTraceEnabled()
    {
        // Arrange
        var logger = new Mock<ILogger>(MockBehavior.Strict);
        logger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
        logger.Setup(l => l.LogTrace("Registering gRPC functions from {0} .proto document", TestProtoPath));

        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(f => f.CreateLogger(typeof(GrpcKernelExtensions))).Returns(logger.Object);

        var kernel = Mock.Of<Kernel>(k => k.LoggerFactory == loggerFactory.Object);
        Mock.Get(kernel).Setup(k => k.CreatePluginFromGrpc(It.IsAny<Stream>(), TestPluginDirName))
            .Returns(Mock.Of<KernelPlugin>());

        Directory.CreateDirectory(Path.Combine(TestParentDir, TestPluginDirName));
        File.WriteAllText(TestProtoPath, "syntax = \"proto3\";");

        // Act
        var result = ((Kernel)kernel).CreatePluginFromGrpcDirectory(TestParentDir, TestPluginDirName);

        // Assert
        logger.Verify(l => l.LogTrace("Registering gRPC functions from {0} .proto document", TestProtoPath), Times.Once);
        logger.VerifyAll();
    }

    [Fact]
    public void CreatePluginFromGrpcDirectory_DoesNotLog_WhenTraceDisabled()
    {
        // Arrange
        var logger = new Mock<ILogger>(MockBehavior.Strict);
        logger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);

        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(f => f.CreateLogger(typeof(GrpcKernelExtensions))).Returns(logger.Object);

        var kernel = Mock.Of<Kernel>(k => k.LoggerFactory == loggerFactory.Object);
        Mock.Get(kernel).Setup(k => k.CreatePluginFromGrpc(It.IsAny<Stream>(), TestPluginDirName))
            .Returns(Mock.Of<KernelPlugin>());

        Directory.CreateDirectory(Path.Combine(TestParentDir, TestPluginDirName));
        File.WriteAllText(TestProtoPath, "syntax = \"proto3\";");

        // Act
        var result = ((Kernel)kernel).CreatePluginFromGrpcDirectory(TestParentDir, TestPluginDirName);

        // Assert
        logger.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        logger.VerifyAll();
    }

    [Fact]
    public void CreatePluginFromGrpcFile_LogsTrace_WhenTraceEnabled()
    {
        // Arrange
        var logger = new Mock<ILogger>(MockBehavior.Strict);
        logger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
        logger.Setup(l => l.LogTrace("Registering gRPC functions from {0} .proto document", TestFilePath));

        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(f => f.CreateLogger(typeof(GrpcKernelExtensions))).Returns(logger.Object);

        var kernel = Mock.Of<Kernel>(k => k.LoggerFactory == loggerFactory.Object);
        Mock.Get(kernel).Setup(k => k.CreatePluginFromGrpc(It.IsAny<Stream>(), TestPluginName))
            .Returns(Mock.Of<KernelPlugin>());

        File.WriteAllText(TestFilePath, "syntax = \"proto3\";");

        // Act
        var result = ((Kernel)kernel).CreatePluginFromGrpcFile(TestFilePath, TestPluginName);

        // Assert
        logger.Verify(l => l.LogTrace("Registering gRPC functions from {0} .proto document", TestFilePath), Times.Once);
        logger.VerifyAll();
    }

    [Fact]
    public void CreatePluginFromGrpcFile_DoesNotLog_WhenTraceDisabled()
    {
        // Arrange
        var logger = new Mock<ILogger>(MockBehavior.Strict);
        logger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);

        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(f => f.CreateLogger(typeof(GrpcKernelExtensions))).Returns(logger.Object);

        var kernel = Mock.Of<Kernel>(k => k.LoggerFactory == loggerFactory.Object);
        Mock.Get(kernel).Setup(k => k.CreatePluginFromGrpc(It.IsAny<Stream>(), TestPluginName))
            .Returns(Mock.Of<KernelPlugin>());

        File.WriteAllText(TestFilePath, "syntax = \"proto3\";");

        // Act
        var result = ((Kernel)kernel).CreatePluginFromGrpcFile(TestFilePath, TestPluginName);

        // Assert
        logger.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        logger.VerifyAll();
    }

    [Fact]
    public void CreatePluginFromGrpcDirectory_ThrowsFileNotFoundException_WhenProtoFileMissing()
    {
        // Arrange
        var loggerFactory = Mock.Of<ILoggerFactory>();
        var kernel = Mock.Of<Kernel>(k => k.LoggerFactory == loggerFactory);

        Directory.CreateDirectory(Path.Combine(TestParentDir, TestPluginDirName));

        // Act & Assert
        var exception = Assert.Throws<FileNotFoundException>(
            () => ((Kernel)kernel).CreatePluginFromGrpcDirectory(TestParentDir, TestPluginDirName));
        Assert.Equal("No .proto document for the specified path - /test/parent/TestPlugin/grpc.proto is found.", exception.Message);
    }

    [Fact]
    public void CreatePluginFromGrpcFile_ThrowsFileNotFoundException_WhenFileMissing()
    {
        // Arrange
        var loggerFactory = Mock.Of<ILoggerFactory>();
        var kernel = Mock.Of<Kernel>(k => k.LoggerFactory == loggerFactory);

        // Act & Assert
        var exception = Assert.Throws<FileNotFoundException>(
            () => ((Kernel)kernel).CreatePluginFromGrpcFile(TestFilePath, TestPluginName));
        Assert.Equal("No .proto document for the specified path - /test.proto is found.", exception.Message);
    }
}
