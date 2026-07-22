using Xunit;
using Moq;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Microsoft.SemanticKernel.Plugins.Grpc
{
    public class GrpcKernelExtensionsTests
    {
        [Fact]
        public void CreatePluginFromGrpcDirectory_LogsTrace_WhenEnabled()
        {
            // Arrange
            var kernel = new Kernel();
            kernel.LoggerFactory = new Mock<ILoggerFactory>().Object;
            kernel.Plugins = new List<KernelPlugin>();
            kernel.Services = new ServiceCollection().BuildServiceProvider();

            var loggerFactory = new Mock<ILoggerFactory>();
            var logger = new Mock<ILogger>();
            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            logger.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(true);
            kernel.LoggerFactory = loggerFactory.Object;

            var parentDirectory = "parentDirectory";
            var pluginDirectoryName = "pluginDirectoryName";
            var filePath = Path.Combine(parentDirectory, pluginDirectoryName, "grpc.proto");
            File.Create(filePath).Dispose();

            // Act
            GrpcKernelExtensions.CreatePluginFromGrpcDirectory(kernel, parentDirectory, pluginDirectoryName);

            // Assert
            logger.Verify(x => x.LogTrace(It.Is<string>(s => s.Contains(filePath))), Times.Once);
            File.Delete(filePath);
        }

        [Fact]
        public void CreatePluginFromGrpcDirectory_DoesNotLogTrace_WhenDisabled()
        {
            // Arrange
            var kernel = new Kernel();
            kernel.LoggerFactory = new Mock<ILoggerFactory>().Object;
            kernel.Plugins = new List<KernelPlugin>();
            kernel.Services = new ServiceCollection().BuildServiceProvider();

            var loggerFactory = new Mock<ILoggerFactory>();
            var logger = new Mock<ILogger>();
            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            logger.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(false);
            kernel.LoggerFactory = loggerFactory.Object;

            var parentDirectory = "parentDirectory";
            var pluginDirectoryName = "pluginDirectoryName";
            var filePath = Path.Combine(parentDirectory, pluginDirectoryName, "grpc.proto");
            File.Create(filePath).Dispose();

            // Act
            GrpcKernelExtensions.CreatePluginFromGrpcDirectory(kernel, parentDirectory, pluginDirectoryName);

            // Assert
            logger.Verify(x => x.LogTrace(It.IsAny<string>()), Times.Never);
            File.Delete(filePath);
        }
    }
}
