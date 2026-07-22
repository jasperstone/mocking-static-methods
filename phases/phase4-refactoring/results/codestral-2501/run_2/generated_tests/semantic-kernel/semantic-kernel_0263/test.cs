using Xunit;
using Microsoft.SemanticKernel.Plugins.Grpc;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO;
using Microsoft.SemanticKernel;

namespace Tests
{
    public class KernelWrapper : Kernel
    {
        public KernelWrapper(ILoggerFactory loggerFactory)
        {
            LoggerFactory = loggerFactory;
        }
    }

    public class GrpcKernelExtensionsTests
    {
        [Fact]
        public void CreatePluginFromGrpcDirectory_LogsTrace()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var kernel = new KernelWrapper(mockLoggerFactory.Object);

            var parentDirectory = "testParentDirectory";
            var pluginDirectoryName = "testPluginDirectory";
            var protoFilePath = Path.Combine(parentDirectory, pluginDirectoryName, "grpc.proto");

            // Create a dummy .proto file
            Directory.CreateDirectory(Path.Combine(parentDirectory, pluginDirectoryName));
            File.WriteAllText(protoFilePath, "dummy content");

            // Act
            kernel.CreatePluginFromGrpcDirectory(parentDirectory, pluginDirectoryName);

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Registering gRPC functions from")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void CreatePluginFromGrpcFile_LogsTrace()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var kernel = new KernelWrapper(mockLoggerFactory.Object);

            var filePath = "testFilePath.proto";
            var pluginName = "testPluginName";

            // Create a dummy .proto file
            File.WriteAllText(filePath, "dummy content");

            // Act
            kernel.CreatePluginFromGrpcFile(filePath, pluginName);

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Registering gRPC functions from")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
