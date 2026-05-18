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

            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            kernelMock.Setup(k => k.LoggerFactory).Returns(loggerFactoryMock.Object);

            var parentDirectory = "parent";
            var pluginDirectoryName = "plugin";
            var protoFilePath = Path.Combine(parentDirectory, pluginDirectoryName, "grpc.proto");

            Directory.CreateDirectory(Path.Combine(parentDirectory, pluginDirectoryName));
            File.WriteAllText(protoFilePath, "proto content");

            kernelMock.Setup(k => k.CreatePluginFromGrpc(It.IsAny<Stream>(), It.IsAny<string>())).Returns(new KernelPlugin());

            // Act
            var result = GrpcKernelExtensions.CreatePluginFromGrpcDirectory(kernelMock.Object, parentDirectory, pluginDirectoryName);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Registering gRPC functions from")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            // Clean up
            File.Delete(protoFilePath);
            Directory.Delete(Path.Combine(parentDirectory, pluginDirectoryName));
            Directory.Delete(parentDirectory);
        }

        [Fact]
        public void CreatePluginFromGrpcFile_ValidFile_LogsTrace()
        {
            // Arrange
            var kernelMock = new Mock<Kernel>();
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            kernelMock.Setup(k => k.LoggerFactory).Returns(loggerFactoryMock.Object);

            var filePath = "test.proto";
            var pluginName = "plugin";

            File.WriteAllText(filePath, "proto content");

            kernelMock.Setup(k => k.CreatePluginFromGrpc(It.IsAny<Stream>(), It.IsAny<string>())).Returns(new KernelPlugin());

            // Act
            var result = GrpcKernelExtensions.CreatePluginFromGrpcFile(kernelMock.Object, filePath, pluginName);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Registering gRPC functions from")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            // Clean up
            File.Delete(filePath);
        }
    }
}
