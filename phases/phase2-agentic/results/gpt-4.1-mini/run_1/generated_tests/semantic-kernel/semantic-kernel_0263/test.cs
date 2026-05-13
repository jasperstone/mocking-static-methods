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

            public override ILoggerFactory LoggerFactory { get; }

            public override IList<KernelPlugin> Plugins { get; }

            public override Microsoft.Extensions.DependencyInjection.IServiceProvider Services { get; }

            public override KernelPlugin CreatePluginFromGrpc(Stream documentStream, string pluginName)
            {
                // Return a dummy plugin for testing
                return new KernelPlugin(pluginName, null, new List<KernelFunction>());
            }
        }

        [Fact]
        public void CreatePluginFromGrpcDirectory_LogsTrace_WhenTraceEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            loggerMock.Setup(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Registering gRPC functions from")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(typeof(GrpcKernelExtensions))).Returns(loggerMock.Object);

            var kernel = new TestKernel(loggerFactoryMock.Object);

            // Setup a temporary directory and file for the test
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var pluginDir = Path.Combine(tempDir, "plugin");
            Directory.CreateDirectory(pluginDir);
            var protoFilePath = Path.Combine(pluginDir, "grpc.proto");
            File.WriteAllText(protoFilePath, "syntax = \"proto3\";");

            try
            {
                // Act
                var plugin = GrpcKernelExtensions.CreatePluginFromGrpcDirectory(kernel, tempDir, "plugin");

                // Assert
                loggerMock.Verify(l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(protoFilePath)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

                Assert.NotNull(plugin);
                Assert.Contains(plugin, kernel.Plugins);
            }
            finally
            {
                // Cleanup
                if (File.Exists(protoFilePath)) File.Delete(protoFilePath);
                if (Directory.Exists(pluginDir)) Directory.Delete(pluginDir);
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir);
            }
        }

        [Fact]
        public void CreatePluginFromGrpcFile_LogsTrace_WhenTraceEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            loggerMock.Setup(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Registering gRPC functions from")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(typeof(GrpcKernelExtensions))).Returns(loggerMock.Object);

            var kernel = new TestKernel(loggerFactoryMock.Object);

            // Setup a temporary file for the test
            var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".proto");
            File.WriteAllText(tempFile, "syntax = \"proto3\";");

            try
            {
                // Act
                var plugin = GrpcKernelExtensions.CreatePluginFromGrpcFile(kernel, tempFile, "pluginName");

                // Assert
                loggerMock.Verify(l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(tempFile)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

                Assert.NotNull(plugin);
                Assert.Contains(plugin, kernel.Plugins);
            }
            finally
            {
                // Cleanup
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }
    }
}
