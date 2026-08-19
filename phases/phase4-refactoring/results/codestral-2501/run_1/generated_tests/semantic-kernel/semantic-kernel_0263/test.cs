using Xunit;
using Microsoft.SemanticKernel.Plugins.Grpc;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO;
using Microsoft.SemanticKernel;

namespace Tests
{
    public class GrpcKernelExtensionsTests
    {
        [Fact]
        public void CreatePluginFromGrpcDirectory_LogsTrace_WhenLoggerIsEnabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(logger => logger.IsEnabled(LogLevel.Trace)).Returns(true);
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(factory => factory.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var mockKernel = new MockKernel();
            mockKernel.LoggerFactory = mockLoggerFactory.Object;

            var parentDirectory = "parent";
            var pluginDirectoryName = "plugin";
            var protoFilePath = Path.Combine(parentDirectory, pluginDirectoryName, "grpc.proto");

            // Create a dummy .proto file
            Directory.CreateDirectory(Path.Combine(parentDirectory, pluginDirectoryName));
            File.WriteAllText(protoFilePath, "dummy content");

            // Act
            var plugin = GrpcKernelExtensions.CreatePluginFromGrpcDirectory(mockKernel, parentDirectory, pluginDirectoryName);

            // Assert
            mockLogger.Verify(logger => logger.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Registering gRPC functions from")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);

            // Clean up
            File.Delete(protoFilePath);
            Directory.Delete(Path.Combine(parentDirectory, pluginDirectoryName));
            Directory.Delete(parentDirectory);
        }

        [Fact]
        public void CreatePluginFromGrpcFile_LogsTrace_WhenLoggerIsEnabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(logger => logger.IsEnabled(LogLevel.Trace)).Returns(true);
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(factory => factory.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var mockKernel = new MockKernel();
            mockKernel.LoggerFactory = mockLoggerFactory.Object;

            var filePath = "path/to/grpc.proto";
            var pluginName = "plugin";

            // Create a dummy .proto file
            File.WriteAllText(filePath, "dummy content");

            // Act
            var plugin = GrpcKernelExtensions.CreatePluginFromGrpcFile(mockKernel, filePath, pluginName);

            // Assert
            mockLogger.Verify(logger => logger.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Registering gRPC functions from")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);

            // Clean up
            File.Delete(filePath);
        }

        private class MockKernel : Kernel
        {
            public new ILoggerFactory LoggerFactory { get; set; }
        }
    }
}
