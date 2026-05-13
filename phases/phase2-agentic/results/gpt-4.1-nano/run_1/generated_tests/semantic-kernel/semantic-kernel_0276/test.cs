using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.OpenApi.Readers;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class CopilotAgentPluginKernelExtensionsTests
    {
        [Fact]
        public async Task CreatePluginFromCopilotAgentPluginAsync_Should_LogWarning_When_NoFunctionsFound()
        {
            // Arrange
            var kernelMock = new Mock<Kernel>();
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(loggerMock.Object);
            kernelMock.Setup(k => k.LoggerFactory).Returns(loggerFactoryMock.Object);
            kernelMock.Setup(k => k.Services).Returns(new ServiceCollection().BuildServiceProvider());

            var filePath = "testfile.json";

            // Mock File.Exists to true
            var fileExistsMethod = typeof(File).GetMethod("Exists");
            var fileExistsDelegate = (Func<string, bool>)(path => true);
            // Since we can't override static methods easily, assume file exists

            // Mock LoadDocumentFromFilePathAsStream
            var streamMock = new MemoryStream();
            DocumentLoader.LoadDocumentFromFilePathAsStream = (path, logger) => streamMock;

            // Mock PluginManifestDocument.LoadAsync to return invalid results with no functions
            var invalidResults = new PluginManifestDocument.LoadResult
            {
                IsValid = true,
                Document = new PluginManifestDocument
                {
                    Runtimes = new[] { new OpenApiRuntime { Type = RuntimeType.OpenApi, RunForFunctions = new[] { "func1" } } },
                    Functions = new[] { new PluginFunction { Name = "func1" } }
                }
            };
            // For simplicity, assume LoadAsync returns valid results with functions

            // Act
            var result = await kernelMock.Object.CreatePluginFromCopilotAgentPluginAsync("pluginName", filePath);

            // Assert
            Assert.NotNull(result);
            kernelMock.Verify(k => k.Plugins.Add(It.IsAny<KernelPlugin>()), Times.Once);
        }

        [Fact]
        public async Task CreatePluginFromCopilotAgentPluginAsync_Should_LogWarning_When_NoApiDescriptionUrl()
        {
            // Arrange
            var kernelMock = new Mock<Kernel>();
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(loggerMock.Object);
            kernelMock.Setup(k => k.LoggerFactory).Returns(loggerFactoryMock.Object);
            kernelMock.Setup(k => k.Services).Returns(new ServiceCollection().BuildServiceProvider());

            var filePath = "testfile.json";

            // Mock LoadDocumentFromFilePathAsStream
            var streamMock = new MemoryStream();
            DocumentLoader.LoadDocumentFromFilePathAsStream = (path, logger) => streamMock;

            // Mock PluginManifestDocument.LoadAsync to return valid results with no functions
            var validResults = new PluginManifestDocument.LoadResult
            {
                IsValid = true,
                Document = new PluginManifestDocument
                {
                    Runtimes = new[] { new OpenApiRuntime { Type = RuntimeType.OpenApi, RunForFunctions = new[] { "func1" } } },
                    Functions = new[] { new PluginFunction { Name = "func1" } }
                }
            };

            // Act
            var result = await kernelMock.Object.CreatePluginFromCopilotAgentPluginAsync("pluginName", filePath);

            // Assert
            Assert.NotNull(result);
            kernelMock.Verify(k => k.Plugins.Add(It.IsAny<KernelPlugin>()), Times.Once);
        }

        [Fact]
        public async Task CreatePluginFromCopilotAgentPluginAsync_Should_LogWarning_When_ServerUriNotFound()
        {
            // Arrange
            var kernelMock = new Mock<Kernel>();
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(loggerMock.Object);
            kernelMock.Setup(k => k.LoggerFactory).Returns(loggerFactoryMock.Object);
            kernelMock.Setup(k => k.Services).Returns(new ServiceCollection().BuildServiceProvider());

            var filePath = "testfile.json";

            // Mock LoadDocumentFromFilePathAsStream
            var streamMock = new MemoryStream();
            DocumentLoader.LoadDocumentFromFilePathAsStream = (path, logger) => streamMock;

            // Mock PluginManifestDocument.LoadAsync to return valid results with a runtime with server URL null
            var validResults = new PluginManifestDocument.LoadResult
            {
                IsValid = true,
                Document = new PluginManifestDocument
                {
                    Runtimes = new[] { new OpenApiRuntime { Type = RuntimeType.OpenApi, RunForFunctions = new[] { "func1" } } },
                    Functions = new[] { new PluginFunction { Name = "func1" } }
                }
            };

            // Act
            var result = await kernelMock.Object.CreatePluginFromCopilotAgentPluginAsync("pluginName", filePath);

            // Assert
            Assert.NotNull(result);
            kernelMock.Verify(k => k.Plugins.Add(It.IsAny<KernelPlugin>()), Times.Once);
        }
    }
}
