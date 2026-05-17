using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Plugins.Grpc;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Grpc.UnitTests
{
    public class GrpcKernelExtensionsTests
    {
        private class TestKernel : Kernel
        {
            public TestKernel(ILoggerFactory loggerFactory)
            {
                this.LoggerFactory = loggerFactory;
                this.Plugins = new List<KernelPlugin>();
                this.Services = new Mock<IServiceProvider>().Object;
            }

            public override List<KernelPlugin> Plugins { get; }

            public override ILoggerFactory LoggerFactory { get; }

            public override IServiceProvider Services { get; }

            public override KernelPlugin CreatePluginFromGrpc(Stream stream, string pluginName)
            {
                return new KernelPlugin(pluginName);
            }
        }

        [Fact]
        public void CreatePluginFromGrpcDirectory_LogsTraceWhenEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(typeof(GrpcKernelExtensions))).Returns(loggerMock.Object);

            var kernel = new TestKernel(loggerFactoryMock.Object);

            // Setup plugin directory and file
            string parentDir = Path.GetTempPath();
            string pluginDirName = Guid.NewGuid().ToString();
            string pluginDir = Path.Combine(parentDir, pluginDirName);
            Directory.CreateDirectory(pluginDir);
            string protoFilePath = Path.Combine(pluginDir, "grpc.proto");
            File.WriteAllText(protoFilePath, "syntax = \"proto3\";");

            // Act
            var ex = Record.Exception(() => GrpcKernelExtensions.CreatePluginFromGrpcDirectory(kernel, parentDir, pluginDirName));

            // Cleanup
            File.Delete(protoFilePath);
            Directory.Delete(pluginDir);

            // Assert
            Assert.Null(ex);
            loggerMock.Verify(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Registering gRPC functions from")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void CreatePluginFromGrpcDirectory_ThrowsFileNotFoundException_WhenProtoFileMissing()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var kernel = new TestKernel(loggerFactoryMock.Object);

            string parentDir = Path.GetTempPath();
            string pluginDirName = Guid.NewGuid().ToString();
            string pluginDir = Path.Combine(parentDir, pluginDirName);
            Directory.CreateDirectory(pluginDir);

            // Act & Assert
            var ex = Assert.Throws<FileNotFoundException>(() =>
                GrpcKernelExtensions.CreatePluginFromGrpcDirectory(kernel, parentDir, pluginDirName));

            // Cleanup
            Directory.Delete(pluginDir);

            Assert.Contains("No .proto document for the specified path", ex.Message);
        }
    }

    // Minimal Kernel and KernelPlugin stubs to allow compilation
    public abstract class Kernel
    {
        public abstract List<KernelPlugin> Plugins { get; }
        public abstract ILoggerFactory LoggerFactory { get; }
        public abstract IServiceProvider Services { get; }
        public virtual KernelPlugin CreatePluginFromGrpc(Stream stream, string pluginName) => new KernelPlugin(pluginName);
    }

    public class KernelPlugin
    {
        public string Name { get; }
        public KernelPlugin(string name) { this.Name = name; }
    }
}
