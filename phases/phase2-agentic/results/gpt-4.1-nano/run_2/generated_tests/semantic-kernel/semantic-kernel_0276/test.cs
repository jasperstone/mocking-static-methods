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
        public async Task CreatePluginFromCopilotAgentPluginAsync_Should_LogWarning_When_ServerUriIsNull()
        {
            // Arrange
            var kernelMock = new Mock<Kernel>();
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(loggerMock.Object);
            kernelMock.Setup(k => k.LoggerFactory).Returns(loggerFactoryMock.Object);
            kernelMock.Setup(k => k.Services).Returns(new ServiceCollection().BuildServiceProvider());

            var pluginFilePath = "test.json";

            // Create a dummy file
            File.WriteAllText(pluginFilePath, "{}");

            // Mock LoadDocumentFromFilePathAsStream to return a stream
            var stream = new MemoryStream();
            var documentLoaderMock = new Mock<IDocumentLoader>();
            documentLoaderMock.Setup(dl => dl.LoadDocumentFromFilePathAsStream(It.IsAny<string>(), It.IsAny<ILogger>()))
                .Returns(stream);

            // Mock PluginManifestDocument.LoadAsync to return invalid results with no errors
            var loadResultMock = new Mock<IPluginManifestLoadResult>();
            loadResultMock.Setup(r => r.IsValid).Returns(true);
            loadResultMock.Setup(r => r.Document).Returns(new PluginManifestDocument
            {
                Runtimes = new[] {
                    new Runtime
                    {
                        Type = RuntimeType.OpenApi,
                        RunForFunctions = new[] { "func1" },
                        Spec = new RuntimeSpec { Url = "http://example.com" }
                    }
                },
                Functions = new[] { new Function { Name = "func1" } }
            });
            var pluginManifestMock = new Mock<IPluginManifestDocument>();
            pluginManifestMock.Setup(pm => pm.LoadAsync(It.IsAny<Stream>(), It.IsAny<ReaderOptions>()))
                .ReturnsAsync(loadResultMock.Object);

            // Act
            await kernelMock.Object.CreatePluginFromCopilotAgentPluginAsync("TestPlugin", pluginFilePath, null, CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
            File.Delete(pluginFilePath);
        }
    }
}
