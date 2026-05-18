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
        public async Task ImportPluginFromCopilotAgentPluginAsync_LogsWarning_WhenNoApiDescriptionUrlFound()
        {
            // Arrange
            var kernelMock = new Mock<Kernel>();
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            kernelMock.Setup(k => k.LoggerFactory).Returns(loggerFactoryMock.Object);

            var pluginName = "TestPlugin";
            var filePath = "path/to/plugin.json";
            var pluginParameters = new CopilotAgentPluginParameters();

            var documentLoaderMock = new Mock<DocumentLoader>();
            documentLoaderMock.Setup(d => d.LoadDocumentFromFilePathAsStream(It.IsAny<string>(), It.IsAny<ILogger>()))
                .Returns(new MemoryStream());

            var pluginManifestDocumentMock = new Mock<PluginManifestDocument>();
            pluginManifestDocumentMock.Setup(p => p.LoadAsync(It.IsAny<Stream>(), It.IsAny<ReaderOptions>()))
                .ReturnsAsync(new PluginManifestDocumentResult
                {
                    IsValid = true,
                    Document = new PluginManifestDocument
                    {
                        Runtimes = new List<Runtime>
                        {
                            new OpenApiRuntime
                            {
                                Type = RuntimeType.OpenApi,
                                RunForFunctions = new List<string> { "Function1" },
                                Spec = new OpenApiSpec { Url = "" }
                            }
                        },
                        Functions = new List<Function>
                        {
                            new Function { Name = "Function1" }
                        }
                    }
                });

            // Act
            await CopilotAgentPluginKernelExtensions.ImportPluginFromCopilotAgentPluginAsync(
                kernelMock.Object,
                pluginName,
                filePath,
                pluginParameters,
                CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No API description URL found in the runtime object.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
