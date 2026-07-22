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
        public async Task ImportPluginFromCopilotAgentPluginAsync_NoFunctionsFound_LogsWarning()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockKernel = new Mock<Kernel>();
            var mockHttpClient = new Mock<HttpClient>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(HttpClient))).Returns(mockHttpClient.Object);
            mockKernel.Setup(k => k.Services).Returns(mockServiceProvider.Object);
            mockKernel.Setup(k => k.LoggerFactory.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var filePath = "path/to/manifest.json";
            var pluginName = "TestPlugin";
            var pluginParameters = new CopilotAgentPluginParameters();

            var manifestContent = "{\"runtimes\": [{\"type\": \"OpenApi\", \"runForFunctions\": []}]}";
            using var manifestStream = new MemoryStream();
            using var writer = new StreamWriter(manifestStream);
            writer.Write(manifestContent);
            writer.Flush();
            manifestStream.Position = 0;

            var mockDocumentLoader = new Mock<DocumentLoader>();
            mockDocumentLoader.Setup(dl => dl.LoadDocumentFromFilePathAsStream(filePath, mockLogger.Object)).Returns(manifestStream);

            // Act
            await CopilotAgentPluginKernelExtensions.ImportPluginFromCopilotAgentPluginAsync(
                mockKernel.Object,
                pluginName,
                filePath,
                pluginParameters,
                CancellationToken.None);

            // Assert
            mockLogger.Verify(logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No functions found in the runtime object.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [Fact]
        public async Task ImportPluginFromCopilotAgentPluginAsync_NoApiDescriptionUrl_LogsWarning()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockKernel = new Mock<Kernel>();
            var mockHttpClient = new Mock<HttpClient>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(HttpClient))).Returns(mockHttpClient.Object);
            mockKernel.Setup(k => k.Services).Returns(mockServiceProvider.Object);
            mockKernel.Setup(k => k.LoggerFactory.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var filePath = "path/to/manifest.json";
            var pluginName = "TestPlugin";
            var pluginParameters = new CopilotAgentPluginParameters();

            var manifestContent = "{\"runtimes\": [{\"type\": \"OpenApi\", \"runForFunctions\": [\"Function1\"], \"spec\": {}}]}";
            using var manifestStream = new MemoryStream();
            using var writer = new StreamWriter(manifestStream);
            writer.Write(manifestContent);
            writer.Flush();
            manifestStream.Position = 0;

            var mockDocumentLoader = new Mock<DocumentLoader>();
            mockDocumentLoader.Setup(dl => dl.LoadDocumentFromFilePathAsStream(filePath, mockLogger.Object)).Returns(manifestStream);

            // Act
            await CopilotAgentPluginKernelExtensions.ImportPluginFromCopilotAgentPluginAsync(
                mockKernel.Object,
                pluginName,
                filePath,
                pluginParameters,
                CancellationToken.None);

            // Assert
            mockLogger.Verify(logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No API description URL found in the runtime object.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}
