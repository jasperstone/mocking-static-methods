using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Plugins.Grpc;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Grpc.Tests
{
    public class GrpcKernelExtensionsTests
    {
        private class TestKernel : Kernel
        {
            public TestKernel(ILoggerFactory loggerFactory)
            {
                this.LoggerFactory = loggerFactory;
                this.Plugins = new List<KernelPlugin>();
                this.Services = new Microsoft.Extensions.DependencyInjection.ServiceCollection().BuildServiceProvider();
            }

            public override List<KernelPlugin> Plugins { get; }

            public override ILoggerFactory LoggerFactory { get; }
        }

        [Fact]
        public void CreatePluginFromGrpcDirectory_LogsTraceWhenEnabled()
        {
            // Arrange
            var parentDir = Path.GetTempPath();
            var pluginDirName = "TestPluginDir";
            var pluginDir = Path.Combine(parentDir, pluginDirName);
            var protoFile = "grpc.proto";
            var protoFilePath = Path.Combine(pluginDir, protoFile);

            Directory.CreateDirectory(pluginDir);
            File.WriteAllText(protoFilePath, "syntax = \"proto3\";");

            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(typeof(GrpcKernelExtensions))).Returns(loggerMock.Object);

            var kernel = new TestKernel(loggerFactoryMock.Object);

            // We need to mock CreatePluginFromGrpc to avoid actual file stream processing
            var kernelMock = new Mock<TestKernel>(loggerFactoryMock.Object) { CallBase = true };
            kernelMock.Setup(k => k.CreatePluginFromGrpc(It.IsAny<Stream>(), pluginDirName))
                .Returns(new KernelPlugin(pluginDirName, null, new List<KernelFunction>()));

            // Act
            var plugin = GrpcKernelExtensions.CreatePluginFromGrpcDirectory(kernelMock.Object, parentDir, pluginDirName);

            // Assert
            loggerMock.Verify(l => l.LogTrace("Registering gRPC functions from {0} .proto document", protoFilePath), Times.Once);

            // Cleanup
            File.Delete(protoFilePath);
            Directory.Delete(pluginDir);
        }

        [Fact]
        public void CreatePluginFromGrpcFile_LogsTraceWhenEnabled()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, "syntax = \"proto3\";");

            var pluginName = "TestPlugin";

            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(typeof(GrpcKernelExtensions))).Returns(loggerMock.Object);

            var kernel = new TestKernel(loggerFactoryMock.Object);

            var kernelMock = new Mock<TestKernel>(loggerFactoryMock.Object) { CallBase = true };
            kernelMock.Setup(k => k.CreatePluginFromGrpc(It.IsAny<Stream>(), pluginName))
                .Returns(new KernelPlugin(pluginName, null, new List<KernelFunction>()));

            // Act
            var plugin = GrpcKernelExtensions.CreatePluginFromGrpcFile(kernelMock.Object, tempFile, pluginName);

            // Assert
            loggerMock.Verify(l => l.LogTrace("Registering gRPC functions from {0} .proto document", tempFile), Times.Once);

            // Cleanup
            File.Delete(tempFile);
        }
    }
}
