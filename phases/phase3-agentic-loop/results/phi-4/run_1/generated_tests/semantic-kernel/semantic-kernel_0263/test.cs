using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using System.IO;
using System;

namespace Microsoft.SemanticKernel.Plugins.Grpc.Tests
{
    // Wrapper class for ILoggerFactory
    public class LoggerFactoryWrapper
    {
        private readonly ILoggerFactory _loggerFactory;

        public LoggerFactoryWrapper(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public ILogger CreateLogger(Type type)
        {
            return _loggerFactory.CreateLogger(type);
        }
    }

    // Mock Kernel class to use LoggerFactoryWrapper
    public class MockKernel
    {
        public LoggerFactoryWrapper LoggerFactory { get; }

        public MockKernel(LoggerFactoryWrapper loggerFactoryWrapper)
        {
            LoggerFactory = loggerFactoryWrapper;
        }

        public void CreatePluginFromGrpcDirectory(string parentDirectory, string pluginDirectoryName)
        {
            const string ProtoFile = "grpc.proto";
            var pluginDir = Path.Combine(parentDirectory, pluginDirectoryName);
            var filePath = Path.Combine(pluginDir, ProtoFile);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"No .proto document for the specified path - {filePath} is found.");
            }

            if (LoggerFactory.CreateLogger(typeof(GrpcKernelExtensions)) is ILogger logger &&
                logger.IsEnabled(LogLevel.Trace))
            {
                logger.LogTrace("Registering gRPC functions from {0} .proto document", filePath);
            }
        }
    }

    public class GrpcKernelExtensionsTests
    {
        [Fact]
        public void CreatePluginFromGrpcDirectory_LogsTraceMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryWrapperMock = new Mock<LoggerFactoryWrapper>();

            // Set up the CreateLogger method on the wrapper
            loggerFactoryWrapperMock
                .Setup(lfw => lfw.CreateLogger(It.IsAny<Type>()))
                .Returns(loggerMock.Object);

            var kernelMock = new MockKernel(loggerFactoryWrapperMock.Object);

            var parentDirectory = "testParentDirectory";
            var pluginDirectoryName = "testPluginDirectory";
            var filePath = Path.Combine(parentDirectory, pluginDirectoryName, "grpc.proto");

            // Ensure the file exists for the test
            Directory.CreateDirectory(filePath);
            File.Create(filePath).Dispose();

            // Act
            kernelMock.CreatePluginFromGrpcDirectory(parentDirectory, pluginDirectoryName);

            // Assert
            loggerMock.Verify(
                l => l.LogTrace(
                    It.Is<string>(s => s == "Registering gRPC functions from {0} .proto document"),
                    It.Is<object[]>(o => o.Length == 1 && o[0].ToString() == filePath)),
                Times.Once);

            // Clean up
            File.Delete(filePath);
            Directory.Delete(Path.GetDirectoryName(filePath));
        }
    }
}
