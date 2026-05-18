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

            // Add the OpenAI chat client with the factory that will call GetService on the service provider
            services.AddOpenAIChatClient(
                modelId,
                endpoint,
                apiKey,
                serviceId: "testService");

            var serviceProvider = services.BuildServiceProvider();

            // Act
            // Resolve the factory delegate registered for the keyed IChatClient
            var factory = serviceProvider.GetService<Func<IServiceProvider, object?, IChatClient>>();
            Assert.NotNull(factory);

            // Call the factory with the mocked IServiceProvider to trigger GetService call
            var chatClient = factory!(mockServiceProvider.Object, null);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.AtLeastOnce());
            Assert.NotNull(chatClient);
        }
    }
}
