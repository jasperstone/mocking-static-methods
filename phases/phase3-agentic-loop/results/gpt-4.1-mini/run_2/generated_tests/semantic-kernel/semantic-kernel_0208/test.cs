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
            var endpoint = new Uri("https://test.endpoint");
            var apiKey = "test-api-key";

            // Add a logger factory to the service collection to avoid null loggerFactory
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            services.AddSingleton(mockLoggerFactory.Object);

            // Act
            services.AddOpenAIChatClient(
                modelId,
                endpoint,
                apiKey,
                serviceId: "testService",
                httpClient: null,
                openTelemetrySourceName: null,
                openTelemetryConfig: null);

            var serviceProvider = services.BuildServiceProvider();

            // Resolve the IChatClient using the factory registered with the serviceId key
            var chatClient = serviceProvider.GetRequiredService<IChatClient>();

            // Assert
            Assert.NotNull(chatClient);
        }
    }
}
