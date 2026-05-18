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
        public void CreatePluginFromGrpcDirectory_LogsTrace_WhenEnabled()
        {
            // Arrange
            var kernel = new Mock<Microsoft.SemanticKernel.Kernel>();
            var loggerFactory = new Mock<ILoggerFactory>();
            kernel.Setup(k => k.LoggerFactory).Returns(loggerFactory.Object);
            var logger = new Mock<ILogger>();
            loggerFactory.Setup(lf => lf.CreateLogger(It.IsAny<Type>())).Returns(logger.Object);
            logger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            var parentDirectory = "parentDirectory";
            var pluginDirectoryName = "pluginDirectoryName";
            var protoFile = "grpc.proto";
            var filePath = Path.Combine(parentDirectory, pluginDirectoryName, protoFile);
            File.Create(filePath).Dispose();

            // Act
            Microsoft.SemanticKernel.Plugins.Grpc.GrpcKernelExtensions.CreatePluginFromGrpcDirectory(kernel.Object, parentDirectory, pluginDirectoryName);

            // Assert
            logger.Verify(l => l.LogTrace(It.Is<string>(s => s.Contains(filePath))), Times.Once);
        }

        [Fact]
        public void CreatePluginFromGrpcFile_LogsTrace_WhenEnabled()
        {
            // Arrange
            var kernel = new Mock<Microsoft.SemanticKernel.Kernel>();
            var loggerFactory = new Mock<ILoggerFactory>();
            kernel.Setup(k => k.LoggerFactory).Returns(loggerFactory.Object);
            var logger = new Mock<ILogger>();
            loggerFactory.Setup(lf => lf.CreateLogger(It.IsAny<Type>())).Returns(logger.Object);
            logger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            var filePath = "filePath";
            File.Create(filePath).Dispose();

            // Act
            Microsoft.SemanticKernel.Plugins.Grpc.GrpcKernelExtensions.CreatePluginFromGrpcFile(kernel.Object, filePath, "pluginName");

            // Assert
            logger.Verify(l => l.LogTrace(It.Is<string>(s => s.Contains(filePath))), Times.Once);
        }
    }
}
