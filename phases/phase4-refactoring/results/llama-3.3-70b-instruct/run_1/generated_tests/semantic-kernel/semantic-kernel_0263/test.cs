using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Plugins.Grpc;
using Moq;
using System;
using System.IO;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class GrpcKernelExtensionsTests
    {
        [Fact]
        public void CreatePluginFromGrpcDirectory_LogsTrace_WhenEnabled()
        {
            // Arrange
            var kernel = new Mock<IKernel>();
            var loggerFactory = new Mock<ILoggerFactory>();
            var logger = new Mock<ILogger>();
            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            kernel.Setup(x => x.LoggerFactory).Returns(loggerFactory.Object);
            logger.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(true);

            var parentDirectory = "parentDirectory";
            var pluginDirectoryName = "pluginDirectoryName";
            var filePath = Path.Combine(parentDirectory, pluginDirectoryName, "grpc.proto");
            File.Create(filePath).Dispose();

            // Act
            GrpcKernelExtensions.CreatePluginFromGrpcDirectory(kernel.Object, parentDirectory, pluginDirectoryName);

            // Assert
            logger.Verify(x => x.LogTrace("Registering gRPC functions from {0} .proto document", filePath), Times.Once);
        }

        [Fact]
        public void CreatePluginFromGrpcFile_LogsTrace_WhenEnabled()
        {
            // Arrange
            var kernel = new Mock<IKernel>();
            var loggerFactory = new Mock<ILoggerFactory>();
            var logger = new Mock<ILogger>();
            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            kernel.Setup(x => x.LoggerFactory).Returns(loggerFactory.Object);
            logger.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(true);

            var filePath = "filePath";
            File.Create(filePath).Dispose();

            // Act
            GrpcKernelExtensions.CreatePluginFromGrpcFile(kernel.Object, filePath, "pluginName");

            // Assert
            logger.Verify(x => x.LogTrace("Registering gRPC functions from {0} .proto document", filePath), Times.Once);
        }
    }
}
