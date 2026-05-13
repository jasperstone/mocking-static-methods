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
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

            kernelMock.Setup(k => k.LoggerFactory.CreateLogger(It.IsAny<Type>())).Returns(loggerMock.Object);

            string parentDirectory = "testParent";
            string pluginDirectoryName = "testPlugin";
            string expectedFilePath = Path.Combine(parentDirectory, pluginDirectoryName, "grpc.proto");

            // Ensure the directory and file exist for the test
            Directory.CreateDirectory(Path.Combine(parentDirectory, pluginDirectoryName));
            File.Create(expectedFilePath).Dispose();

            // Act
            kernelMock.Object.CreatePluginFromGrpcDirectory(parentDirectory, pluginDirectoryName);

            // Assert
            loggerMock.Verify(
                l => l.LogTrace("Registering gRPC functions from {0} .proto document", expectedFilePath),
                Times.Once);
        }
    }
}
