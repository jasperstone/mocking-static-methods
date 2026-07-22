using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Plugins.Grpc;
using Xunit;
using Moq;

namespace Microsoft.SemanticKernel.Plugins.Grpc.Tests
{
    public class GrpcKernelExtensionsTests
    {
        [Fact]
        public void CreatePluginFromGrpcDirectory_ThrowsFileNotFoundException_WhenProtoFileMissing()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var kernelMock = new Mock<Kernel>();
            kernelMock.SetupGet(k => k.LoggerFactory).Returns(loggerFactoryMock.Object);
            kernelMock.SetupGet(k => k.Plugins).Returns(new List<KernelPlugin>());

            string parentDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            string pluginDirName = "MissingPlugin";

            // Act & Assert
            var ex = Assert.Throws<FileNotFoundException>(() =>
                GrpcKernelExtensions.CreatePluginFromGrpcDirectory(kernelMock.Object, parentDir, pluginDirName));
            Assert.Contains("No .proto document for the specified path", ex.Message);
        }

        [Fact]
        public void CreatePluginFromGrpcFile_ThrowsFileNotFoundException_WhenFileMissing()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var kernelMock = new Mock<Kernel>();
            kernelMock.SetupGet(k => k.LoggerFactory).Returns(loggerFactoryMock.Object);
            kernelMock.SetupGet(k => k.Plugins).Returns(new List<KernelPlugin>());

            string filePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "missing.proto");

            // Act & Assert
            var ex = Assert.Throws<FileNotFoundException>(() =>
                GrpcKernelExtensions.CreatePluginFromGrpcFile(kernelMock.Object, filePath, "TestPlugin"));
            Assert.Contains("No .proto document for the specified path", ex.Message);
        }
    }
}
