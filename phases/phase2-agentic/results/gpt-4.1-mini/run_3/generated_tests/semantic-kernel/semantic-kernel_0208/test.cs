using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOpenAIChatClient_WithUri_CallsGetServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "test-model";
            var endpoint = new Uri("https://test.endpoint/");
            var apiKey = "test-api-key";

            // Mock ILoggerFactory to be returned by IServiceProvider.GetService<ILoggerFactory>()
            var mockLoggerFactory = new Mock<ILoggerFactory>();

            // Mock IServiceProvider to verify GetService<ILoggerFactory> is called
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(mockLoggerFactory.Object)
                .Verifiable();

            // Add the OpenAI chat client with a factory that uses the mocked IServiceProvider
            services.AddOpenAIChatClient(
                modelId,
                endpoint,
                apiKey,
                httpClient: null,
                openTelemetrySourceName: null,
                openTelemetryConfig: null);

            var serviceProvider = services.BuildServiceProvider();

            // Act
            // Resolve the IChatClient using the factory registered in the service collection
            var chatClientFactory = serviceProvider.GetRequiredService<Func<IServiceProvider, object?, IChatClient>>();
            var chatClient = chatClientFactory(mockServiceProvider.Object, null);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.AtLeastOnce());
            Assert.NotNull(chatClient);
        }
    }
}
