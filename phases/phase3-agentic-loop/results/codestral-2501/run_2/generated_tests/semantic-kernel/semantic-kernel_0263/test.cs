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
        public void CreatePluginFromGrpcDirectory_LogsTrace()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var kernel = new Kernel();
            kernel.LoggerFactory = mockLoggerFactory.Object;

            var parentDirectory = "parent";
            var pluginDirectoryName = "plugin";
            var protoFilePath = Path.Combine(parentDirectory, pluginDirectoryName, "grpc.proto");

            // Create a dummy .proto file
            Directory.CreateDirectory(Path.Combine(parentDirectory, pluginDirectoryName));
            File.WriteAllText(protoFilePath, "dummy content");

            // Act
            kernel.CreatePluginFromGrpcDirectory(parentDirectory, pluginDirectoryName);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Trace),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Registering gRPC functions from")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
