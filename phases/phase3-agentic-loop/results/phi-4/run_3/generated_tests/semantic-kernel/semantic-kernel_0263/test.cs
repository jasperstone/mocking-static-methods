using Moq;
using Microsoft.Extensions.Logging;
using System.IO;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Grpc.Tests
{
    public class GrpcKernelExtensionsTests
    {
        [Fact]
        public void CreatePluginFromGrpcDirectory_LogsTraceMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            // Directly return the logger mock without using the extension method
            loggerFactoryMock
                .Setup(lf => lf.CreateLogger(It.IsAny<string>()))
                .Returns(loggerMock.Object);

            var kernelMock = new Mock<IKernel>();
            kernelMock
                .SetupGet(k => k.LoggerFactory)
                .Returns(loggerFactoryMock.Object);

            var parentDirectory = "testParentDirectory";
            var pluginDirectoryName = "testPluginDirectory";
            var filePath = Path.Combine(parentDirectory, pluginDirectoryName, "grpc.proto");

            // Ensure the file exists for the test
            Directory.CreateDirectory(filePath);
            File.Create(filePath).Dispose();

            // Act
            kernelMock.Object.CreatePluginFromGrpcDirectory(parentDirectory, pluginDirectoryName);

            // Assert
            loggerMock.Verify(
                l => l.LogTrace(
                    It.Is<string>(s => s == "Registering gRPC functions from {0} .proto document"),
                    It.Is<object[]>(o => o[0].ToString() == filePath)),
                Times.Once);

            // Clean up
            Directory.Delete(filePath, true);
        }
    }
}
