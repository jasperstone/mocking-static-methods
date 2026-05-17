using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Microsoft.SemanticKernel.Tests
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOpenAITextEmbeddingGeneration_AddsServiceToCollection()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "modelId";
            var openAIClient = new OpenAIClient(new HttpClient(), "apiKey");
            var serviceId = "serviceId";
            var dimensions = 128;

            // Act
            services.AddOpenAITextEmbeddingGeneration(modelId, openAIClient, serviceId, dimensions);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textEmbeddingGenerationService = serviceProvider.GetService<ITextEmbeddingGenerationService>();
            Assert.NotNull(textEmbeddingGenerationService);
        }

        [Fact]
        public void AddOpenAITextEmbeddingGeneration_ThrowsException_WhenServicesIsNull()
        {
            // Arrange
            IServiceCollection services = null;
            var modelId = "modelId";
            var openAIClient = new OpenAIClient(new HttpClient(), "apiKey");
            var serviceId = "serviceId";
            var dimensions = 128;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => services.AddOpenAITextEmbeddingGeneration(modelId, openAIClient, serviceId, dimensions));
        }

        [Fact]
        public void AddOpenAITextEmbeddingGeneration_ThrowsException_WhenModelIdIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            string modelId = null;
            var openAIClient = new OpenAIClient(new HttpClient(), "apiKey");
            var serviceId = "serviceId";
            var dimensions = 128;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => services.AddOpenAITextEmbeddingGeneration(modelId, openAIClient, serviceId, dimensions));
        }
    }
}
