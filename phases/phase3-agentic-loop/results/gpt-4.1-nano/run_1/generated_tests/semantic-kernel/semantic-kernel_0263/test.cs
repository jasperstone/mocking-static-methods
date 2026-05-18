using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Grpc;

namespace SemanticKernel.Tests
{
    public class GrpcKernelExtensionsTests
    {
        [Fact]
        public void CreatePluginFromGrpcDirectory_LogsTrace_WhenLoggerEnabled()
        {
            // Arrange
            var kernelMock = new Mock<Kernel>();
            var plugins = new List<KernelPlugin>();
            kernelMock.Setup(k => k.Plugins).Returns(plugins);
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(loggerMock.Object);
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            kernelMock.Setup(k => k.LoggerFactory).Returns(loggerFactoryMock.Object);
            kernelMock.Setup(k => k.CreatePluginFromGrpc(It.IsAny<Stream>(), It.IsAny<string>()))
                .Returns(new KernelPlugin());

            // Setup file system
            var parentDir = Path.GetTempPath();
            var pluginDirName = "TestPlugin";
            var pluginDirPath = Path.Combine(parentDir, pluginDirName);
            Directory.CreateDirectory(pluginDirPath);
            var protoFilePath = Path.Combine(pluginDirPath, "grpc.proto");
            File.WriteAllText(protoFilePath, "dummy proto content");

            // Act
            var result = GrpcKernelExtensions.CreatePluginFromGrpcDirectory(kernelMock.Object, parentDir, pluginDirName);

            // Assert
            Assert.Contains(result, plugins);
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
            // Cleanup
            File.Delete(protoFilePath);
            Directory.Delete(pluginDirPath);
        }
    }
}
