using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Grpc;
using Microsoft.SemanticKernel.Plugins.Grpc.Extensions;

namespace SemanticKernel.Tests
{
    public class GrpcKernelExtensionsTests
    {
        [Fact]
        public void CreatePluginFromGrpcDirectory_LogsTrace_WhenLoggerEnabled()
        {
            // Arrange
            var kernelMock = new Mock<Kernel>();
            var plugins = new PluginCollection();
            kernelMock.Setup(k => k.Plugins).Returns(plugins);
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(loggerMock.Object);
            kernelMock.Setup(k => k.LoggerFactory).Returns(loggerFactoryMock.Object);
            var servicesMock = new Mock<IServiceProvider>();
            kernelMock.Setup(k => k.Services).Returns(servicesMock.Object);

            string parentDir = Path.GetTempPath();
            string pluginDirName = "TestPlugin";

            // Create dummy grpc.proto file
            string pluginDirPath = Path.Combine(parentDir, pluginDirName);
            Directory.CreateDirectory(pluginDirPath);
            string protoFilePath = Path.Combine(pluginDirPath, "grpc.proto");
            File.WriteAllText(protoFilePath, "syntax = \"proto3\";");

            // Act
            var result = GrpcKernelExtensions.CreatePluginFromGrpcDirectory(kernelMock.Object, parentDir, pluginDirName);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.Is<string>(msg => msg.Contains("Registering gRPC functions from")), It.IsAny<object[]>()), Times.Once);

            // Cleanup
            File.Delete(protoFilePath);
            Directory.Delete(pluginDirPath);
        }

        [Fact]
        public void CreatePluginFromGrpcFile_LogsTrace_WhenLoggerEnabled()
        {
            // Arrange
            var kernelMock = new Mock<Kernel>();
            var plugins = new PluginCollection();
            kernelMock.Setup(k => k.Plugins).Returns(plugins);
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(loggerMock.Object);
            kernelMock.Setup(k => k.LoggerFactory).Returns(loggerFactoryMock.Object);

            string tempFilePath = Path.GetTempFileName();
            File.WriteAllText(tempFilePath, "syntax = \"proto3\";");

            // Act
            var plugin = GrpcKernelExtensions.CreatePluginFromGrpcFile(kernelMock.Object, tempFilePath, "TestPlugin");

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.Is<string>(msg => msg.Contains("Registering gRPC functions from")), It.IsAny<object[]>()), Times.Once);

            // Cleanup
            File.Delete(tempFilePath);
        }
    }
}
