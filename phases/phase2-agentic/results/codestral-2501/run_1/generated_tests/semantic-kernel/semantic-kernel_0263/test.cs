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
        [Fact]
        public void CreatePluginFromGrpcDirectory_ValidDirectory_LogsTrace()
        {
            // Arrange
            var kernelMock = new Mock<Kernel>();
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            kernelMock.Setup(x => x.LoggerFactory).Returns(loggerFactoryMock.Object);

            var parentDirectory = "validParentDirectory";
            var pluginDirectoryName = "validPluginDirectory";
            var protoFilePath = Path.Combine(parentDirectory, pluginDirectoryName, "grpc.proto");

            Directory.CreateDirectory(Path.Combine(parentDirectory, pluginDirectoryName));
            File.WriteAllText(protoFilePath, "proto content");

            // Act
            GrpcKernelExtensions.CreatePluginFromGrpcDirectory(kernelMock.Object, parentDirectory, pluginDirectoryName);

            // Assert
            loggerMock.Verify(x => x.LogTrace(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Func<It.IsAnyType, Exception, string>>(), It.IsAny<Exception>()), Times.Once);
        }

        [Fact]
        public void CreatePluginFromGrpcDirectory_InvalidDirectory_ThrowsFileNotFoundException()
        {
            // Arrange
            var kernelMock = new Mock<Kernel>();
            var parentDirectory = "invalidParentDirectory";
            var pluginDirectoryName = "invalidPluginDirectory";

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => GrpcKernelExtensions.CreatePluginFromGrpcDirectory(kernelMock.Object, parentDirectory, pluginDirectoryName));
        }

        [Fact]
        public void CreatePluginFromGrpcFile_ValidFile_LogsTrace()
        {
            // Arrange
            var kernelMock = new Mock<Kernel>();
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            kernelMock.Setup(x => x.LoggerFactory).Returns(loggerFactoryMock.Object);

            var filePath = "validFilePath.proto";
            var pluginName = "validPluginName";

            File.WriteAllText(filePath, "proto content");

            // Act
            GrpcKernelExtensions.CreatePluginFromGrpcFile(kernelMock.Object, filePath, pluginName);

            // Assert
            loggerMock.Verify(x => x.LogTrace(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Func<It.IsAnyType, Exception, string>>(), It.IsAny<Exception>()), Times.Once);
        }

        [Fact]
        public void CreatePluginFromGrpcFile_InvalidFile_ThrowsFileNotFoundException()
        {
            // Arrange
            var kernelMock = new Mock<Kernel>();
            var filePath = "invalidFilePath.proto";
            var pluginName = "invalidPluginName";

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => GrpcKernelExtensions.CreatePluginFromGrpcFile(kernelMock.Object, filePath, pluginName));
        }
    }
}
