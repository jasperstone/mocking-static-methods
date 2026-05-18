using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Plugins.Grpc;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Grpc.Tests
{
    public class GrpcKernelExtensionsTests
    {
        private class KernelWrapper
        {
            public Kernel Kernel { get; }
            public KernelWrapper(Kernel kernel)
            {
                Kernel = kernel;
            }

            public KernelPlugin CreatePluginFromGrpcDirectory(string parentDirectory, string pluginDirectoryName)
            {
                return GrpcKernelExtensions.CreatePluginFromGrpcDirectory(Kernel, parentDirectory, pluginDirectoryName);
            }

            public KernelPlugin CreatePluginFromGrpcFile(string filePath, string pluginName)
            {
                return GrpcKernelExtensions.CreatePluginFromGrpcFile(Kernel, filePath, pluginName);
            }
        }

        [Fact]
        public void CreatePluginFromGrpcDirectory_LogsTraceMessage()
        {
            // Arrange
            var kernelMock = new Mock<Kernel>();
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(lf => lf.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

            kernelMock.Setup(k => k.LoggerFactory).Returns(loggerFactoryMock.Object);

            var kernelWrapper = new KernelWrapper(kernelMock.Object);
            var parentDirectory = "parent";
            var pluginDirectoryName = "plugin";
            var filePath = Path.Combine(parentDirectory, pluginDirectoryName, "grpc.proto");

            // Act
            kernelWrapper.CreatePluginFromGrpcDirectory(parentDirectory, pluginDirectoryName);

            // Assert
            loggerMock.Verify(l => l.LogTrace("Registering gRPC functions from {0} .proto document", filePath), Times.Once);
        }

        [Fact]
        public void CreatePluginFromGrpcDirectory_ThrowsFileNotFoundException_WhenFileDoesNotExist()
        {
            // Arrange
            var kernelMock = new Mock<Kernel>();
            var kernelWrapper = new KernelWrapper(kernelMock.Object);
            var parentDirectory = "parent";
            var pluginDirectoryName = "plugin";

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => kernelWrapper.CreatePluginFromGrpcDirectory(parentDirectory, pluginDirectoryName));
        }

        [Fact]
        public void CreatePluginFromGrpcFile_LogsTraceMessage()
        {
            // Arrange
            var kernelMock = new Mock<Kernel>();
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(lf => lf.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

            kernelMock.Setup(k => k.LoggerFactory).Returns(loggerFactoryMock.Object);

            var kernelWrapper = new KernelWrapper(kernelMock.Object);
            var filePath = "path/to/grpc.proto";

            // Act
            kernelWrapper.CreatePluginFromGrpcFile(filePath, "pluginName");

            // Assert
            loggerMock.Verify(l => l.LogTrace("Registering gRPC functions from {0} .proto document", filePath), Times.Once);
        }

        [Fact]
        public void CreatePluginFromGrpcFile_ThrowsFileNotFoundException_WhenFileDoesNotExist()
        {
            // Arrange
            var kernelMock = new Mock<Kernel>();
            var kernelWrapper = new KernelWrapper(kernelMock.Object);
            var filePath = "path/to/grpc.proto";

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => kernelWrapper.CreatePluginFromGrpcFile(filePath, "pluginName"));
        }
    }
}
