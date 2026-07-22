using Xunit;
using Microsoft.SemanticKernel.Plugins.Grpc;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO;
using Microsoft.SemanticKernel;

namespace GrpcKernelExtensionsTests
{
    public class KernelWrapper
    {
        private readonly Kernel _kernel;
        private readonly ILogger _logger;

        public KernelWrapper(Kernel kernel, ILogger logger)
        {
            _kernel = kernel;
            _logger = logger;
        }

        public ILoggerFactory LoggerFactory => _kernel.LoggerFactory;

        public KernelPlugin CreatePluginFromGrpc(Stream documentStream, string pluginName)
        {
            return _kernel.CreatePluginFromGrpc(documentStream, pluginName);
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _logger;
        }
    }

    public class GrpcKernelExtensionsTests
    {
        [Fact]
        public void CreatePluginFromGrpcDirectory_LogsTrace()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockKernel = new Mock<Kernel>();
            var kernelWrapper = new KernelWrapper(mockKernel.Object, mockLogger.Object);

            var parentDirectory = "parent";
            var pluginDirectoryName = "plugin";
            var protoFilePath = Path.Combine(parentDirectory, pluginDirectoryName, "grpc.proto");

            // Create a dummy .proto file
            Directory.CreateDirectory(Path.Combine(parentDirectory, pluginDirectoryName));
            File.WriteAllText(protoFilePath, "dummy content");

            // Act
            GrpcKernelExtensions.CreatePluginFromGrpcDirectory(kernelWrapper, parentDirectory, pluginDirectoryName);

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
            var mockKernel = new Mock<Kernel>();
            var kernelWrapper = new KernelWrapper(mockKernel.Object, mockLogger.Object);

            var filePath = "path/to/grpc.proto";

            // Create a dummy .proto file
            File.WriteAllText(filePath, "dummy content");

            // Act
            GrpcKernelExtensions.CreatePluginFromGrpcFile(kernelWrapper, filePath, "pluginName");

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
