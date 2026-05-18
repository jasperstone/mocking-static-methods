using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
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
        public async Task ImportPluginFromCopilotAgentPluginAsync_LogsWarning_WhenNoApiDescriptionUrl()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernel = new Mock<Kernel>();
            kernel.Setup(k => k.LoggerFactory).Returns(new LoggerFactory().AddProvider(new TestLoggerProvider(loggerMock.Object)));
            var pluginName = "TestPlugin";
            var filePath = "path/to/plugin";
            var pluginParameters = new CopilotAgentPluginParameters();
            var cancellationToken = CancellationToken.None;

            // Mock the DocumentLoader to return a document with no OpenAPI runtime
            var document = new PluginManifestDocument
            {
                Runtimes = new List<Runtime>
                {
                    new OpenApiRuntime
                    {
                        Type = RuntimeType.OpenApi,
                        RunForFunctions = new List<string>(),
                        Spec = new OpenApiSpec { Url = string.Empty }
                    }
                }
            };

            var results = new PluginManifestDocumentLoadResult
            {
                IsValid = true,
                Document = document
            };

            var copilotAgentFileJsonContents = new MemoryStream();
            var documentLoaderMock = new Mock<DocumentLoader>();
            documentLoaderMock.Setup(dl => dl.LoadDocumentFromFilePathAsStream(filePath, It.IsAny<ILogger>()))
                .Returns(copilotAgentFileJsonContents);

            var pluginManifestDocumentMock = new Mock<PluginManifestDocument>();
            pluginManifestDocumentMock.Setup(pmd => pmd.LoadAsync(It.IsAny<Stream>(), It.IsAny<ReaderOptions>()))
                .ReturnsAsync(results);

            // Act
            await kernel.Object.ImportPluginFromCopilotAgentPluginAsync(pluginName, filePath, pluginParameters, cancellationToken);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No API description URL found in the runtime object.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    public class TestLoggerProvider : ILoggerProvider
    {
        private readonly ILogger _logger;

        public TestLoggerProvider(ILogger logger)
        {
            _logger = logger;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _logger;
        }

        public void Dispose()
        {
        }
    }
}
