using Xunit;
using Moq;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Microsoft.SemanticKernel.Plugins.Grpc.Tests
{
    public class GrpcKernelExtensionsTests
    {
        [Fact]
        public void CreatePluginFromGrpcDirectory_LogsTraceMessage_WhenEnabled()
        {
            // Arrange
            var kernel = new Mock<Kernel>();
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
            logger.Verify(x => x.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);

            File.Delete(filePath);
        }

        [Fact]
        public void CreatePluginFromGrpcDirectory_DoesNotLogTraceMessage_WhenDisabled()
        {
            // Arrange
            var kernel = new Mock<Kernel>();
            var loggerFactory = new Mock<ILoggerFactory>();
            var logger = new Mock<ILogger>();
            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            kernel.Setup(x => x.LoggerFactory).Returns(loggerFactory.Object);
            logger.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(false);

            var parentDirectory = "parentDirectory";
            var pluginDirectoryName = "pluginDirectoryName";
            var filePath = Path.Combine(parentDirectory, pluginDirectoryName, "grpc.proto");
            File.Create(filePath).Dispose();

            // Act
            GrpcKernelExtensions.CreatePluginFromGrpcDirectory(kernel.Object, parentDirectory, pluginDirectoryName);

            // Assert
            logger.Verify(x => x.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);

            File.Delete(filePath);
        }
    }
}
