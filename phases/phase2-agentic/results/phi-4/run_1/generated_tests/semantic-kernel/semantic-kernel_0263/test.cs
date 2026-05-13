using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Grpc.Tests
{
    public class GrpcKernelExtensionsTests
    {
        [Fact]
        public void CreatePluginFromGrpcDirectory_LogsTraceMessage_WhenLoggerIsEnabled()
        {
            // Arrange
            var kernelMock = new Mock<Kernel>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(loggerMock.Object);

            kernelMock.Setup(k => k.LoggerFactory).Returns(loggerFactoryMock.Object);
            kernelMock.Setup(k => k.Plugins).Returns(new System.Collections.Generic.List<string>());

            var parentDirectory = "testParent";
            var pluginDirectoryName = "testPlugin";
            var pluginDir = Path.Combine(parentDirectory, pluginDirectoryName);
            var filePath = Path.Combine(pluginDir, "grpc.proto");

            Directory.CreateDirectory(pluginDir);
            File.WriteAllText(filePath, "proto content");

            // Act
            GrpcKernelExtensions.CreatePluginFromGrpcDirectory(kernelMock.Object, parentDirectory, pluginDirectoryName);

            // Assert
            loggerMock.Verify(l => l.LogTrace("Registering gRPC functions from {0} .proto document", filePath), Times.Once);

            // Cleanup
            Directory.Delete(pluginDir, true);
        }

        [Fact]
        public void CreatePluginFromGrpcDirectory_DoesNotLogTraceMessage_WhenLoggerIsDisabled()
        {
            // Arrange
            var kernelMock = new Mock<Kernel>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(loggerMock.Object);

            kernelMock.Setup(k => k.LoggerFactory).Returns(loggerFactoryMock.Object);
            kernelMock.Setup(k => k.Plugins).Returns(new System.Collections.Generic.List<string>());

            var parentDirectory = "testParent";
            var pluginDirectoryName = "testPlugin";
            var pluginDir = Path.Combine(parentDirectory, pluginDirectoryName);
            var filePath = Path.Combine(pluginDir, "grpc.proto");

            Directory.CreateDirectory(pluginDir);
            File.WriteAllText(filePath, "proto content");

            // Act
            GrpcKernelExtensions.CreatePluginFromGrpcDirectory(kernelMock.Object, parentDirectory, pluginDirectoryName);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);

            // Cleanup
            Directory.Delete(pluginDir, true);
        }
    }
}
