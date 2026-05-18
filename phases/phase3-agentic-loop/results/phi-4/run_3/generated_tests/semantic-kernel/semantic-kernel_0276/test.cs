using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class CopilotAgentPluginKernelExtensionsTests
    {
        [Fact]
        public async Task ImportPluginFromCopilotAgentPluginAsync_LogsWarning_WhenNoApiDescriptionUrl()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelMock = new Mock<Kernel>();
            var pluginName = "TestPlugin";
            var filePath = "path/to/plugin";
            var pluginParameters = new CopilotAgentPluginParameters();
            var cancellationToken = CancellationToken.None;

            kernelMock.Setup(k => k.LoggerFactory.CreateLogger(It.IsAny<string>()))
                      .Returns(loggerMock.Object);

            // Simulate the scenario where no API description URL is found
            var openApiRuntimeMock = new Mock<OpenApiRuntime>();
            openApiRuntimeMock.Setup(r => r.Spec.Url).Returns(string.Empty);

            var runtimeMock = new Mock<Runtime>();
            runtimeMock.Setup(r => r.Type).Returns(RuntimeType.OpenApi);
            runtimeMock.Setup(r => r as OpenApiRuntime).Returns(openApiRuntimeMock.Object);

            var documentMock = new Mock<PluginManifestDocument>();
            documentMock.Setup(d => d.Runtimes).Returns(new List<Runtime> { runtimeMock.Object });

            var resultsMock = new Mock<PluginManifestLoadResult>();
            resultsMock.Setup(r => r.IsValid).Returns(true);
            resultsMock.Setup(r => r.Document).Returns(documentMock.Object);

            var documentLoaderMock = new Mock<DocumentLoader>();
            documentLoaderMock.Setup(d => d.LoadDocumentFromFilePathAsStream(filePath, It.IsAny<ILogger>()))
                              .Returns(new MemoryStream());

            var pluginManifestDocumentMock = new Mock<PluginManifestDocument>();
            pluginManifestDocumentMock.Setup(p => p.LoadAsync(It.IsAny<Stream>(), It.IsAny<ReaderOptions>()))
                                      .ReturnsAsync(resultsMock.Object);

            // Act
            await CopilotAgentPluginKernelExtensions.ImportPluginFromCopilotAgentPluginAsync(
                kernelMock.Object,
                pluginName,
                filePath,
                pluginParameters,
                cancellationToken);

            // Assert
            loggerMock.Verify(l => l.LogWarning("No API description URL found in the runtime object."), Times.Once);
        }
    }
}
