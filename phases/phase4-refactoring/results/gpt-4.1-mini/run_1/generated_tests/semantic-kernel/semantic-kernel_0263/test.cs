using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Grpc;
using Xunit;
using Moq;

namespace Microsoft.SemanticKernel.Plugins.Grpc.Tests
{
    public class GrpcKernelExtensionsTests
    {
        [Fact]
        public void CreatePluginFromGrpcDirectory_LogsTraceWhenEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(typeof(GrpcKernelExtensions))).Returns(loggerMock.Object);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            var kernel = new Kernel(serviceProviderMock.Object);

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
                loggerMock.Verify(l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(protoFilePath)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
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
        public void CreatePluginFromGrpcFile_LogsTraceWhenEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(typeof(GrpcKernelExtensions))).Returns(loggerMock.Object);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            var kernel = new Kernel(serviceProviderMock.Object);

            string tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, "syntax = \"proto3\";");

            try
            {
                // Act
                var plugin = GrpcKernelExtensions.CreatePluginFromGrpcFile(kernel, tempFile, "TestPlugin");

                // Assert
                loggerMock.Verify(l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(tempFile)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }
    }
}
