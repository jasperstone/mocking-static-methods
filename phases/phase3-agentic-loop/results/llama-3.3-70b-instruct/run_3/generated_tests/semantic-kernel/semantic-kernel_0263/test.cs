using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.IO;
using System;
using Microsoft.SemanticKernel;

namespace Functions.Grpc.Tests
{
    public class GrpcKernelExtensionsTests
    {
        [Fact]
        public void CreatePluginFromGrpcDirectory_ValidInput_LogsTrace()
        {
            // Arrange
            var kernelMock = new Mock<Kernel>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            loggerFactoryMock.Setup(lf => lf.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            kernelMock.Setup(k => k.LoggerFactory).Returns(loggerFactoryMock.Object);

            var parentDirectory = "parentDirectory";
            var pluginDirectoryName = "pluginDirectoryName";
            var protoFile = "grpc.proto";
            var filePath = Path.Combine(parentDirectory, pluginDirectoryName, protoFile);

            // Act
            Microsoft.SemanticKernel.Plugins.Grpc.Extensions.GrpcKernelExtensions.CreatePluginFromGrpcDirectory(kernelMock.Object, parentDirectory, pluginDirectoryName);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.Is<string>(s => s.Contains(filePath))), Times.Once);
        }

        [Fact]
        public void CreatePluginFromGrpcDirectory_InvalidInput_ThrowsFileNotFoundException()
        {
            // Arrange
            var kernelMock = new Mock<Kernel>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            loggerFactoryMock.Setup(lf => lf.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            kernelMock.Setup(k => k.LoggerFactory).Returns(loggerFactoryMock.Object);

            var parentDirectory = "parentDirectory";
            var pluginDirectoryName = "pluginDirectoryName";
            var protoFile = "grpc.proto";
            var filePath = Path.Combine(parentDirectory, pluginDirectoryName, protoFile);

            // Act and Assert
            Assert.Throws<FileNotFoundException>(() => Microsoft.SemanticKernel.Plugins.Grpc.Extensions.GrpcKernelExtensions.CreatePluginFromGrpcDirectory(kernelMock.Object, parentDirectory, pluginDirectoryName));
        }
    }
}
