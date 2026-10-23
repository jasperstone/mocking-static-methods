using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Services;
using Microsoft.Plugins.Manifest;
using Microsoft.SemanticKernel.Http;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using Microsoft.SemanticKernel.Plugins.OpenApi.Extensions;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class CopilotAgentPluginKernelExtensionsTests
    {
        [Fact]
        public async Task ImportPluginFromCopilotAgentPluginAsync_NoApiDescriptionUrl_LogsWarning()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var kernel = new Kernel();
            kernel.LoggerFactory = new NullLoggerFactory();

            var filePath = "path/to/manifest.json";
            var pluginName = "TestPlugin";
            var pluginParameters = new CopilotAgentPluginParameters();

            var manifestDocument = new PluginManifestDocument
            {
                Runtimes = new List<Runtime>
                {
                    new OpenApiRuntime
                    {
                        Type = RuntimeType.OpenApi,
                        Spec = new OpenApiSpec { Url = string.Empty }
                    }
                }
            };

            var results = new PluginManifestDocumentLoadResult
            {
                IsValid = true,
                Document = manifestDocument
            };

            var mockDocumentLoader = new Mock<DocumentLoader>();
            mockDocumentLoader.Setup(dl => dl.LoadDocumentFromFilePathAsStream(It.IsAny<string>(), It.IsAny<ILogger>()))
                .Returns(new MemoryStream());

            var mockPluginManifestDocument = new Mock<PluginManifestDocument>();
            mockPluginManifestDocument.Setup(pmd => pmd.LoadAsync(It.IsAny<Stream>(), It.IsAny<ReaderOptions>()))
                .ReturnsAsync(results);

            // Act
            await CopilotAgentPluginKernelExtensions.ImportPluginFromCopilotAgentPluginAsync(
                kernel,
                pluginName,
                filePath,
                pluginParameters,
                CancellationToken.None);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No API description URL found in the runtime object.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }
    }
}
