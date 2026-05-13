using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.TextGeneration;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests.Connectors.Ollama.Extensions
{
    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaChatCompletion_CallsGetServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "test-model";
            var endpoint = new Uri("http://localhost");

            var mockLoggerFactory = new Mock<ILoggerFactory>();

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(mockLoggerFactory.Object)
                .Verifiable();

            // We need to simulate the AddKeyedSingleton extension method behavior.
            // Since it's not defined here, we simulate the delegate call directly.

            // Act
            var serviceCollection = services.AddOllamaChatCompletion(modelId, endpoint);

            // Extract the factory delegate from the service descriptor
            var serviceDescriptor = Assert.Single(serviceCollection, d => d.ServiceType == typeof(IChatCompletionService));
            var factory = serviceDescriptor.ImplementationFactory;
            Assert.NotNull(factory);

            // Invoke the factory delegate with our mocked service provider
            var serviceInstance = factory(mockServiceProvider.Object);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.AtLeastOnce());
            Assert.NotNull(serviceInstance);
            Assert.IsAssignableFrom<IChatCompletionService>(serviceInstance);
        }
    }
}
