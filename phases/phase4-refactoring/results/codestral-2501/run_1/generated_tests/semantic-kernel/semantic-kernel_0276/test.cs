using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class CopilotAgentPluginKernelExtensionsTests
    {
        [Fact]
        public async Task ImportPluginFromCopilotAgentPluginAsync_LogsWarning_WhenNoApiDescriptionUrlFound()
        {
            // Arrange
            var mockKernel = new Mock<Kernel>();
            var mockLogger = new Mock<ILogger>();
            var mockHttpClient = new Mock<HttpClient>();
            var mockDocumentLoader = new Mock<DocumentLoader>();
            var mockOpenApiStreamReader = new Mock<OpenApiStreamReader>();
            var mockOpenApiFilterService = new Mock<OpenApiFilterService>();

            var pluginName = "TestPlugin";
            var filePath = "testPath";
            var pluginParameters = new CopilotAgentPluginParameters();

            var cancellationToken = CancellationToken.None;

            mockKernel.Setup(k => k.LoggerFactory.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);
            mockKernel.Setup(k => k.Services.GetService<HttpClient>()).Returns(mockHttpClient.Object);

            var manifestDocument = new PluginManifestDocument
            {
                Runtimes = new List<Runtime>
                {
                    new OpenApiRuntime
                    {
                        Type = RuntimeType.OpenApi,
                        Spec = new OpenApiSpec { Url = "" }
                    }
                },
                Functions = new List<Function>
                {
                    new Function { Name = "TestFunction" }
                }
            };

            var results = new PluginManifestDocumentLoadResult
            {
                IsValid = true,
                Document = manifestDocument
            };

            mockDocumentLoader.Setup(dl => dl.LoadDocumentFromFilePathAsStream(It.IsAny<string>(), It.IsAny<ILogger>()))
                .Returns(new MemoryStream());

            mockOpenApiStreamReader.Setup(reader => reader.ReadAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new OpenApiStreamReaderResult
                {
                    OpenApiDocument = new OpenApiDocument(),
                    OpenApiDiagnostic = new OpenApiDiagnostic()
                });

            // Act
            await CopilotAgentPluginKernelExtensions.ImportPluginFromCopilotAgentPluginAsync(
                mockKernel.Object,
                pluginName,
                filePath,
                pluginParameters,
                cancellationToken);

            // Assert
            mockLogger.Verify(
                logger => logger.LogWarning("No API description URL found in the runtime object."),
                Times.Once);
        }
    }
}
