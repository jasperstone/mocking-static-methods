using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;

namespace SemanticKernel.Tests
{
    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaChatCompletion_WithUriAndModel_ShouldRegisterServiceAndCallGetService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Mock IServiceProvider to return ILoggerFactory
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object);

            // Setup the chain for builder
            var builderMock = new Mock<IKernelBuilder>();
            var chatCompletionServiceMock = new Mock<IChatCompletionService>();
            var chatClientMock = new Mock<IChatClient>();
            var builderUseLoggingCalled = false;

            chatClientMock.Setup(c => c.AsBuilder()).Returns(builderMock.Object);
            builderMock.Setup(b => b.UseLogging(It.IsAny<ILoggerFactory>()))
                .Callback(() => builderUseLoggingCalled = true)
                .Returns(builderMock.Object);
            builderMock.Setup(b => b.UseKernelFunctionInvocation(It.IsAny<ILoggerFactory>()))
                .Returns(builderMock.Object);
            builderMock.Setup(b => b.Build(It.IsAny<IServiceProvider>()))
                .Returns(new Mock<IKernel>().Object);
            builderMock.Setup(b => b.AsChatCompletionService())
                .Returns(chatCompletionServiceMock.Object);

            // Setup the IServiceCollection to return the builder chain
            services.AddSingleton<IChatClient>(chatClientMock.Object);

            // Act
            var result = services.AddOllamaChatCompletion("modelId", new Uri("http://endpoint"));

            // Assert
            Assert.Contains(result, s => s == services);
            // Verify that GetService<ILoggerFactory> was called
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.AtLeastOnce);
            // Verify that UseLogging was called
            Assert.True(builderUseLoggingCalled);
        }
    }
}
