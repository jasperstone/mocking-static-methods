using Xunit;
using Moq;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Plugins.Grpc;

namespace Microsoft.SemanticKernel.Plugins.Grpc.Tests
{
    public class GrpcKernelExtensionsTests
    {
        [Fact]
        public void CreatePluginFromGrpcDirectory_ValidDirectoryAndPluginName_ReturnsKernelPlugin()
        {
            // Arrange
            var kernel = new Mock<Kernel>();
            var parentDirectory = "parentDirectory";
            var pluginDirectoryName = "pluginDirectoryName";
            var filePath = Path.Combine(parentDirectory, pluginDirectoryName, "grpc.proto");

            var kernelPlugin = new Mock<KernelPlugin>();
            kernel.Setup(k => k.CreatePluginFromGrpc(It.IsAny<Stream>(), It.IsAny<string>()))
                .Returns(kernelPlugin.Object);

            // Act
            var plugin = GrpcKernelExtensions.CreatePluginFromGrpcDirectory(kernel.Object, parentDirectory, pluginDirectoryName);

            // Assert
            Assert.NotNull(plugin);
        }

        [Fact]
        public void CreatePluginFromGrpcDirectory_InvalidDirectory_ThrowsFileNotFoundException()
        {
            // Arrange
            var kernel = new Mock<Kernel>();
            var parentDirectory = "invalidDirectory";
            var pluginDirectoryName = "pluginDirectoryName";

            // Act and Assert
            Assert.Throws<FileNotFoundException>(() => GrpcKernelExtensions.CreatePluginFromGrpcDirectory(kernel.Object, parentDirectory, pluginDirectoryName));
        }

        [Fact]
        public void CreatePluginFromGrpcFile_ValidFilePathAndPluginName_ReturnsKernelPlugin()
        {
            // Arrange
            var kernel = new Mock<Kernel>();
            var filePath = "filePath";
            var pluginName = "pluginName";

            var kernelPlugin = new Mock<KernelPlugin>();
            kernel.Setup(k => k.CreatePluginFromGrpc(It.IsAny<Stream>(), It.IsAny<string>()))
                .Returns(kernelPlugin.Object);

            // Act
            var plugin = GrpcKernelExtensions.CreatePluginFromGrpcFile(kernel.Object, filePath, pluginName);

            // Assert
            Assert.NotNull(plugin);
        }

        [Fact]
        public void CreatePluginFromGrpcFile_InvalidFilePath_ThrowsFileNotFoundException()
        {
            // Arrange
            var kernel = new Mock<Kernel>();
            var filePath = "invalidFilePath";
            var pluginName = "pluginName";

            // Act and Assert
            Assert.Throws<FileNotFoundException>(() => GrpcKernelExtensions.CreatePluginFromGrpcFile(kernel.Object, filePath, pluginName));
        }

        [Fact]
        public void CreatePluginFromGrpcDirectory_TraceLoggingEnabled_LogsTraceMessage()
        {
            // Arrange
            var kernel = new Mock<Kernel>();
            var parentDirectory = "parentDirectory";
            var pluginDirectoryName = "pluginDirectoryName";
            var filePath = Path.Combine(parentDirectory, pluginDirectoryName, "grpc.proto");

            var loggerFactory = new Mock<ILoggerFactory>();
            var logger = new Mock<ILogger>();
            logger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

            loggerFactory.Setup(lf => lf.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

            kernel.Setup(k => k.LoggerFactory).Returns(loggerFactory.Object);

            // Act
            GrpcKernelExtensions.CreatePluginFromGrpcDirectory(kernel.Object, parentDirectory, pluginDirectoryName);

            // Assert
            logger.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
