using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using Moq;
using OpenAI;
using Xunit;

namespace Microsoft.SemanticKernel.Tests.Connectors.OpenAI.Extensions
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOpenAITextEmbeddingGeneration_CallsGetServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "test-model";
            var mockOpenAIClient = new Mock<OpenAIClient>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService(typeof(OpenAIClient))).Returns(mockOpenAIClient.Object);

            // Act
            var result = services.AddOpenAITextEmbeddingGeneration(modelId, openAIClient: null, serviceId: "testService", dimensions: 123);

            // Extract the factory delegate from the service registration
            var serviceDescriptor = Assert.Single(result, sd => sd.ServiceType == typeof(ITextEmbeddingGenerationService));
            var factory = serviceDescriptor.ImplementationFactory;
            Assert.NotNull(factory);

            // Invoke the factory delegate with the mocked service provider
            var embeddingService = factory(mockServiceProvider.Object);

            // Assert
            Assert.NotNull(embeddingService);
            Assert.IsType<OpenAITextEmbeddingGenerationService>(embeddingService);

            // Verify that GetService<ILoggerFactory>() was called on the service provider
            mockServiceProvider.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
        }
    }
}
