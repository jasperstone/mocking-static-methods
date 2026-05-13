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

            var mockLoggerFactory = new Mock<ILoggerFactory>();

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);

            var endpoint = new Uri("https://example.com");
            var modelId = "test-model";

            // We will capture the factory delegate registered in the service collection
            IServiceProvider capturedServiceProvider = null!;
            Func<IServiceProvider, object?, IChatClient>? factory = null;

            services.AddKeyedSingleton<IChatClient>(
                null,
                (sp, _) =>
                {
                    capturedServiceProvider = sp;
                    return Mock.Of<IChatClient>();
                });

            // Act
            services.AddOpenAIChatClient(
                modelId,
                endpoint,
                apiKey: "test-api-key",
                serviceId: null,
                httpClient: null,
                openTelemetrySourceName: null,
                openTelemetryConfig: null);

            var provider = services.BuildServiceProvider();

            // We get the factory from the service collection descriptors
            var descriptor = Assert.Single(services, d => d.ServiceType == typeof(IChatClient));
            factory = descriptor.ImplementationFactory as Func<IServiceProvider, object?, IChatClient>;
            Assert.NotNull(factory);

            // Act - invoke the factory with our mock service provider
            var chatClient = factory!(mockServiceProvider.Object, null);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.AtLeastOnce());
            Assert.NotNull(chatClient);
        }
    }
}
