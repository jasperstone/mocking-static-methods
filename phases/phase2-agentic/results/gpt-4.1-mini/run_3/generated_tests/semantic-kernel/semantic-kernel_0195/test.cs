using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using OpenAI;
using Xunit;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;

namespace Microsoft.SemanticKernel.Tests.Connectors.OpenAI.Extensions
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOpenAITextEmbeddingGeneration_WithOpenAIClient_UsesGetServiceForLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockOpenAIClient = new Mock<OpenAIClient>(MockBehavior.Strict, new object[] { new OpenAI.OpenAIClientOptions() });

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(OpenAIClient))).Throws(new Exception("Should not be called"));

            services.AddSingleton(serviceProviderMock.Object);

            // We need to add the OpenAIClient to the service collection to satisfy the call if openAIClient is null
            services.AddSingleton(mockOpenAIClient.Object);

            // Act
            var result = services.AddOpenAITextEmbeddingGeneration(
                modelId: "test-model",
                openAIClient: mockOpenAIClient.Object,
                serviceId: "test-service",
                dimensions: 123);

            // Assert
            Assert.Same(services, result);

            // Now resolve the service to trigger the factory delegate and verify GetService was called
            var sp = services.BuildServiceProvider();

            var embeddingService = sp.GetService<ITextEmbeddingGenerationService>();
            Assert.NotNull(embeddingService);

            // Verify that GetService<ILoggerFactory> was called on the service provider mock
            serviceProviderMock.Verify(spm => spm.GetService(typeof(ILoggerFactory)), Times.AtLeastOnce);
        }

        [Fact]
        public void AddOpenAITextEmbeddingGeneration_WithoutOpenAIClient_UsesGetRequiredServiceForOpenAIClient()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockOpenAIClient = new Mock<OpenAIClient>(MockBehavior.Strict, new object[] { new OpenAI.OpenAIClientOptions() });

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(OpenAIClient))).Returns(mockOpenAIClient.Object);

            services.AddSingleton(serviceProviderMock.Object);
            services.AddSingleton(mockOpenAIClient.Object);

            // Act
            var result = services.AddOpenAITextEmbeddingGeneration(
                modelId: "test-model",
                openAIClient: null,
                serviceId: "test-service",
                dimensions: 123);

            // Assert
            Assert.Same(services, result);

            var sp = services.BuildServiceProvider();

            var embeddingService = sp.GetService<ITextEmbeddingGenerationService>();
            Assert.NotNull(embeddingService);

            serviceProviderMock.Verify(spm => spm.GetRequiredService(typeof(OpenAIClient)), Times.AtLeastOnce);
            serviceProviderMock.Verify(spm => spm.GetService(typeof(ILoggerFactory)), Times.AtLeastOnce);
        }
    }
}
