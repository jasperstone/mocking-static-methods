using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Plugins.OpenApi.Extensions;
using Microsoft.SemanticKernel;

namespace SemanticKernel.Tests
{
    public class CopilotAgentPluginKernelExtensionsTests
    {
        [Fact]
        public async Task CreatePluginFromCopilotAgentPluginAsync_ShouldLogWarnings_ForMissingFunctionsAndUrls()
        {
            // Arrange
            var kernelMock = new Mock<Kernel>();
            var loggerMock = new Mock<ILogger>();
            var pluginParameters = new CopilotAgentPluginParameters();

            // Setup kernel.Services to return a HttpClient
            var serviceCollection = new ServiceCollection();
            var httpClient = new HttpClient();
            serviceCollection.AddSingleton(httpClient);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            kernelMock.Setup(k => k.Services).Returns(serviceProvider);
            kernelMock.Setup(k => k.LoggerFactory).Returns(new LoggerFactory());
            kernelMock.Setup(k => k.LoggerFactory.CreateLogger(It.IsAny<Type>())).Returns(loggerMock.Object);
            // Use the actual collection type for Plugins
            var plugins = new System.Collections.Generic.List<KernelPlugin>();
            kernelMock.Setup(k => k.Plugins).Returns(plugins);
            // Mock CreatePluginFromCopilotAgentPluginAsync to return a dummy KernelPlugin
            kernelMock.Setup(k => k.CreatePluginFromCopilotAgentPluginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CopilotAgentPluginParameters?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Mock<KernelPlugin>().Object);

            // Act
            await kernelMock.Object.CreatePluginFromCopilotAgentPluginAsync(
                "TestPlugin",
                "dummyPath.json",
                pluginParameters,
                CancellationToken.None);

            // Assert
            // Verify that logger.LogWarning was called at least once with a message containing specific substrings
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No functions found in the runtime object.") ||
                                                    v.ToString().Contains("No API description URL found in the runtime object.") ||
                                                    v.ToString().Contains("Server URI not found")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.AtLeastOnce);
        }
    }
}
