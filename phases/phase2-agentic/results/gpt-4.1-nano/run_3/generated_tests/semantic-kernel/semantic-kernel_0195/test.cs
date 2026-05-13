using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.TextToImage;
using Microsoft.SemanticKernel.Embeddings;

namespace SemanticKernel.Tests
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOpenAITextEmbeddingGeneration_WithServiceProvider_GetServiceCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new MockServiceProvider();

            // Register a dummy OpenAIClient to satisfy GetRequiredService
            services.AddSingleton(new OpenAIClient());

            // Act
            services.AddOpenAITextEmbeddingGeneration(
                modelId: "text-embedding-ada-002",
                apiKey: "test-api-key",
                orgId: "org-test",
                serviceId: "test-service",
                dimensions: 128);

            // Build service provider
            var provider = services.BuildServiceProvider();

            // Act: resolve the service to trigger the GetService call
            var service = provider.GetService<ITextEmbeddingGenerationService>();

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void AddOpenAITextToImage_WithServiceProvider_GetServiceCalled()
        {
            // Arrange
            var services = new ServiceCollection();

            // Register a dummy HttpClientProvider
            services.AddSingleton<HttpClientProvider>();

            // Register a dummy ILoggerFactory
            services.AddLogging();

            // Act
            services.AddOpenAITextToImage(
                apiKey: "test-api-key",
                orgId: "org-test",
                modelId: "dall-e",
                serviceId: "test-service");

            var provider = services.BuildServiceProvider();

            // Act
            var service = provider.GetService<ITextToImageService>();

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void AddOpenAITextToAudio_WithServiceProvider_GetServiceCalled()
        {
            // Arrange
            var services = new ServiceCollection();

            // Register a dummy HttpClientProvider
            services.AddSingleton<HttpClientProvider>();

            // Register a dummy ILoggerFactory
            services.AddLogging();

            // Act
            services.AddOpenAITextToAudio(
                modelId: "whisper-1",
                apiKey: "test-api-key",
                orgId: "org-test");

            var provider = services.BuildServiceProvider();

            // Act
            var service = provider.GetService<ITextToAudioService>();

            // Assert
            Assert.NotNull(service);
        }
    }

    // Dummy implementation for testing GetService call
    public class MockServiceProvider : IServiceProvider
    {
        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(ILoggerFactory))
            {
                return new LoggerFactory();
            }
            if (serviceType == typeof(OpenAIClient))
            {
                return new OpenAIClient();
            }
            return null;
        }
    }
}
