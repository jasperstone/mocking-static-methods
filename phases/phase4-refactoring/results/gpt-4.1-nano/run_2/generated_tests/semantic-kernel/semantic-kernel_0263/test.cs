using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Grpc;
using Microsoft.Extensions.DependencyInjection;

namespace SemanticKernel.Tests
{
    public class GrpcKernelExtensionsTests
    {
        [Fact]
        public void CreatePluginFromGrpcDirectory_LogsTrace_WhenLoggerEnabled()
        {
            // Arrange
            var pluginName = "testPlugin";
            var parentDir = Path.GetTempPath();
            var pluginDirName = "testPluginDir";

            var pluginDirPath = Path.Combine(parentDir, pluginDirName);
            Directory.CreateDirectory(pluginDirPath);
            var protoFilePath = Path.Combine(pluginDirPath, "grpc.proto");
            File.WriteAllText(protoFilePath, "dummy proto content");

            var plugin = new Mock<KernelPlugin>().Object;

            var kernelMock = new Mock<Kernel>();
            var plugins = new List<KernelPlugin>();
            kernelMock.Setup(k => k.Plugins).Returns(plugins);
            kernelMock.Setup(k => k.CreatePluginFromGrpc(It.IsAny<Stream>(), It.IsAny<string>()))
                .Returns(plugin);

            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(typeof(GrpcKernelExtensions)))
                .Returns(loggerMock.Object);

            var services = new ServiceCollection();
            services.AddHttpClient();
            var serviceProvider = services.BuildServiceProvider();

            kernelMock.Setup(k => k.LoggerFactory).Returns(loggerFactoryMock.Object);
            kernelMock.Setup(k => k.Services).Returns(serviceProvider);

            // Act
            var result = GrpcKernelExtensions.CreatePluginFromGrpcDirectory(
                kernelMock.Object,
                parentDir,
                pluginDirName);

            // Assert
            Assert.Contains(plugin, plugins);
            loggerMock.Verify(l => l.LogTrace(It.Is<string>(s => s.Contains("Registering gRPC functions from")), It.IsAny<object[]>()), Times.Once);

            // Cleanup
            File.Delete(protoFilePath);
            Directory.Delete(pluginDirPath);
        }
    }
}
