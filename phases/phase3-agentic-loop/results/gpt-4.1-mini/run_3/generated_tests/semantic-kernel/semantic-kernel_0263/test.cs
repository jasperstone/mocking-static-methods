using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Plugins.Grpc;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Grpc.Tests
{
    public class GrpcKernelExtensionsTests
    {
        private class TestKernel : IKernel
        {
            public TestKernel(ILoggerFactory loggerFactory)
            {
                this.LoggerFactory = loggerFactory;
                this.Plugins = new List<KernelPlugin>();
                this.Services = new ServiceCollection().BuildServiceProvider();
            }

            public IList<KernelPlugin> Plugins { get; }

            public ILoggerFactory LoggerFactory { get; }

            public IServiceProvider Services { get; }

            public KernelPlugin CreatePluginFromGrpc(Stream documentStream, string pluginName)
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

            // Setup directory and file structure for test
            string parentDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            string pluginDirName = "TestPlugin";
            string pluginDir = Path.Combine(parentDir, pluginDirName);
            Directory.CreateDirectory(pluginDir);
            string protoFilePath = Path.Combine(pluginDir, "grpc.proto");
            File.WriteAllText(protoFilePath, "syntax = \"proto3\";");

            try
            {
                // Act
                var plugin = GrpcKernelExtensions.CreatePluginFromGrpcDirectory(kernel, parentDir, pluginDirName);

                // Assert
                loggerMock.Verify(l => l.IsEnabled(LogLevel.Trace), Times.Once);
                loggerMock.Verify(l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(protoFilePath)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
            }
            finally
            {
                // Cleanup
                if (File.Exists(protoFilePath)) File.Delete(protoFilePath);
                if (Directory.Exists(pluginDir)) Directory.Delete(pluginDir);
                if (Directory.Exists(parentDir)) Directory.Delete(parentDir);
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

            // Setup file for test
            string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".proto");
            File.WriteAllText(tempFile, "syntax = \"proto3\";");

            try
            {
                // Act
                var plugin = GrpcKernelExtensions.CreatePluginFromGrpcFile(kernel, tempFile, "TestPlugin");

                // Assert
                loggerMock.Verify(l => l.IsEnabled(LogLevel.Trace), Times.Once);
                loggerMock.Verify(l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(tempFile)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
            }
            finally
            {
                // Cleanup
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }
    }

    // Minimal interfaces and classes to support the test
    public interface IKernel
    {
        IList<KernelPlugin> Plugins { get; }
        ILoggerFactory LoggerFactory { get; }
        IServiceProvider Services { get; }
        KernelPlugin CreatePluginFromGrpc(Stream documentStream, string pluginName);
    }

    public class KernelPlugin
    {
        public KernelPlugin(string name, object? something, IList<KernelFunction> functions)
        {
            this.Name = name;
            this.Functions = functions;
        }

        public string Name { get; }
        public IList<KernelFunction> Functions { get; }
    }

    public class KernelFunction
    {
    }
}
