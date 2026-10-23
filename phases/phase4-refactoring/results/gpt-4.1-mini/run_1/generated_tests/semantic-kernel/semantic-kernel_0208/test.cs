using System;
using System.Linq;
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

            var modelId = "test-model";
            var endpoint = new Uri("https://test.endpoint/");
            var apiKey = "test-api-key";

            var httpClient = new HttpClient { BaseAddress = endpoint };

            // Act
            services.AddOpenAIChatClient(
                modelId,
                endpoint,
                apiKey,
                httpClient: httpClient);

            // Retrieve the factory delegate registered in the service collection
            var serviceDescriptor = services
                .FirstOrDefault(sd => sd.ImplementationFactory != null);

            Assert.NotNull(serviceDescriptor);

            // The factory is a Func<IServiceProvider, IChatClient> cast to object
            var factory = serviceDescriptor.ImplementationFactory;
            Assert.NotNull(factory);

            // Invoke the factory with the mocked service provider
            var chatClient = factory!(mockServiceProvider.Object);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.AtLeastOnce);
            Assert.NotNull(chatClient);
        }
    }
}
