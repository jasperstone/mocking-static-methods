using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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
        public async Task ImportPluginFromCopilotAgentPluginAsync_LogsWarning_WhenNoApiDescriptionUrl()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernel = new Mock<Kernel>();
            kernel.Setup(k => k.LoggerFactory).Returns(new LoggerFactory().AddProvider(new TestLoggerProvider(loggerMock.Object)));
            kernel.Setup(k => k.Services.GetService<IServiceProvider>()).Returns(new ServiceCollection().BuildServiceProvider());
            kernel.Setup(k => k.Services.GetService<HttpClient>()).Returns(new HttpClient());

            var pluginName = "TestPlugin";
            var filePath = "path/to/plugin.json";
            var pluginParameters = new CopilotAgentPluginParameters();

            var document = new PluginManifestDocument
            {
                Runtimes = new List<Runtime>
                {
                    new OpenApiRuntime
                    {
                        RunForFunctions = new List<string> { "Function1" },
                        Spec = new OpenApiSpec { Url = string.Empty }
                    }
                },
                Functions = new List<Function>
                {
                    new Function { Name = "Function1" }
                }
            };

            var results = new PluginManifestDocumentLoadResult
            {
                IsValid = true,
                Document = document
            };

            var documentStream = new MemoryStream();
            using (var writer = new StreamWriter(documentStream, leaveOpen: true))
            {
                writer.Write("{}");
            }
            documentStream.Position = 0;

            var documentLoaderMock = new Mock<DocumentLoader>();
            documentLoaderMock.Setup(dl => dl.LoadDocumentFromFilePathAsStream(filePath, It.IsAny<ILogger>()))
                .Returns(documentStream);

            var openApiStreamReaderMock = new Mock<OpenApiStreamReader>();
            openApiStreamReaderMock.Setup(oars => oars.ReadAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new OpenApiStreamReaderResult
                {
                    OpenApiDocument = new OpenApiDocument(),
                    OpenApiDiagnostic = new OpenApiDiagnostic()
                });

            // Act
            await kernel.Object.ImportPluginFromCopilotAgentPluginAsync(pluginName, filePath, pluginParameters);

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

        public ILogger CreateLogger(string categoryName) => _logger;

        public void Dispose() { }
    }
}
