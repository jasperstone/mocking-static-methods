using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO;
using System;

namespace Microsoft.SemanticKernel.Plugins.Grpc;

public class GrpcKernelExtensionsTests
{
    [Fact]
    public void CreatePluginFromGrpcDirectory_LogsTrace_WhenEnabled()
    {
        // Arrange
        var kernel = new Mock<Kernel>();
        var loggerFactory = new Mock<ILoggerFactory>();
        var logger = new Mock<ILogger>();
        logger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(logger.Object);
        kernel.Setup(k => k.LoggerFactory).Returns(loggerFactory.Object);

        var parentDirectory = "parentDirectory";
        var pluginDirectoryName = "pluginDirectoryName";
        var filePath = Path.Combine(parentDirectory, pluginDirectoryName, "grpc.proto");

        // Act
        GrpcKernelExtensions.CreatePluginFromGrpcDirectory(kernel.Object, parentDirectory, pluginDirectoryName);

        // Assert
        logger.Verify(l => l.LogTrace("Registering gRPC functions from {0} .proto document", filePath), Times.Once);
    }

    [Fact]
    public void CreatePluginFromGrpcFile_LogsTrace_WhenEnabled()
    {
        // Arrange
        var kernel = new Mock<Kernel>();
        var loggerFactory = new Mock<ILoggerFactory>();
        var logger = new Mock<ILogger>();
        logger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(logger.Object);
        kernel.Setup(k => k.LoggerFactory).Returns(loggerFactory.Object);

        var filePath = "filePath";
        var pluginName = "pluginName";

        // Act
        GrpcKernelExtensions.CreatePluginFromGrpcFile(kernel.Object, filePath, pluginName);

        // Assert
        logger.Verify(l => l.LogTrace("Registering gRPC functions from {0} .proto document", filePath), Times.Once);
    }
}
