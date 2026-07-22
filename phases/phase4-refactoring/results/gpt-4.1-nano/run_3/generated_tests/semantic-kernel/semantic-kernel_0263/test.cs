using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Grpc.Tests
{
    public class GrpcKernelExtensionsTests
    {
        [Fact]
        public void CreatePluginFromGrpcDirectory_LogsTrace_WhenLoggerEnabled()
        {
            // Arrange
            var pluginName = "TestPlugin";
            var parentDirectory = Path.GetTempPath();
            var pluginDirName = "TestPluginDir";

            var pluginDirPath = Path.Combine(parentDirectory, pluginDirName);
            Directory.CreateDirectory(pluginDirPath);

            var protoFilePath = Path.Combine(pluginDirPath, "grpc.proto");
            File.WriteAllText(protoFilePath, "syntax = \"proto3\";");

            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(true);
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<Type>())).Returns(mockLogger.Object);

            var mockKernel = new Mock<Kernel>();
            var pluginCollection = new List<KernelPlugin>();
            mockKernel.SetupGet(k => k.Plugins).Returns(pluginCollection);
            mockKernel.SetupGet(k => k.LoggerFactory).Returns(mockLoggerFactory.Object);
            var mockPlugin = new Mock<KernelPlugin>();
            mockKernel.Setup(k => k.CreatePluginFromGrpc(It.IsAny<Stream>(), It.IsAny<string>()))
                .Returns(mockPlugin.Object);

            // Act
            var result = GrpcKernelExtensions.CreatePluginFromGrpcDirectory(
                mockKernel.Object,
                parentDirectory,
                pluginDirName);

            // Assert
            mockLogger.Verify(
                x => x.LogTrace("Registering gRPC functions from {0} .proto document", It.IsAny<string>()),
                Times.Once);

            // Cleanup
            Directory.Delete(pluginDirPath, true);
        }
    }
}
