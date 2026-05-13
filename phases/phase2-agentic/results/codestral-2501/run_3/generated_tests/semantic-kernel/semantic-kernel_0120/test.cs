using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Http;
using Microsoft.SemanticKernel.ImageToText;
using Microsoft.SemanticKernel.TextGeneration;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHuggingFaceTextGeneration_WithModel_ShouldAddService()
        {
            // Arrange
            var services = new ServiceCollection();
            var model = "test-model";
            var endpoint = new Uri("https://api.huggingface.com");
            var apiKey = "test-api-key";
            var serviceId = "test-service-id";
            var httpClient = new HttpClient();

            // Act
            services.AddHuggingFaceTextGeneration(model, endpoint, apiKey, serviceId, httpClient);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var service = serviceProvider.GetService<ITextGenerationService>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddHuggingFaceTextGeneration_WithEndpoint_ShouldAddService()
        {
            // Arrange
            var services = new ServiceCollection();
            var endpoint = new Uri("https://api.huggingface.com");
            var apiKey = "test-api-key";
            var serviceId = "test-service-id";
            var httpClient = new HttpClient();

            // Act
            services.AddHuggingFaceTextGeneration(endpoint, apiKey, serviceId, httpClient);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var service = serviceProvider.GetService<ITextGenerationService>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddHuggingFaceChatCompletion_WithModel_ShouldAddService()
        {
            // Arrange
            var services = new ServiceCollection();
            var model = "test-model";
            var endpoint = new Uri("https://api.huggingface.com");
            var apiKey = "test-api-key";
            var serviceId = "test-service-id";
            var httpClient = new HttpClient();

            // Act
            services.AddHuggingFaceChatCompletion(model, endpoint, apiKey, serviceId, httpClient);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var service = serviceProvider.GetService<IChatCompletionService>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddHuggingFaceChatCompletion_WithEndpoint_ShouldAddService()
        {
            // Arrange
            var services = new ServiceCollection();
            var endpoint = new Uri("https://api.huggingface.com");
            var apiKey = "test-api-key";
            var serviceId = "test-service-id";
            var httpClient = new HttpClient();

            // Act
            services.AddHuggingFaceChatCompletion(endpoint, apiKey, serviceId, httpClient);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var service = serviceProvider.GetService<IChatCompletionService>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddHuggingFaceTextEmbeddingGeneration_WithModel_ShouldAddService()
        {
            // Arrange
            var services = new ServiceCollection();
            var model = "test-model";
            var endpoint = new Uri("https://api.huggingface.com");
            var apiKey = "test-api-key";
            var serviceId = "test-service-id";
            var httpClient = new HttpClient();

            // Act
            services.AddHuggingFaceTextEmbeddingGeneration(model, endpoint, apiKey, serviceId, httpClient);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var service = serviceProvider.GetService<ITextEmbeddingGenerationService>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddHuggingFaceTextEmbeddingGeneration_WithEndpoint_ShouldAddService()
        {
            // Arrange
            var services = new ServiceCollection();
            var endpoint = new Uri("https://api.huggingface.com");
            var apiKey = "test-api-key";
            var serviceId = "test-service-id";
            var httpClient = new HttpClient();

            // Act
            services.AddHuggingFaceTextEmbeddingGeneration(endpoint, apiKey, serviceId, httpClient);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var service = serviceProvider.GetService<ITextEmbeddingGenerationService>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddHuggingFaceEmbeddingGenerator_WithModel_ShouldAddService()
        {
            // Arrange
            var services = new ServiceCollection();
            var model = "test-model";
            var endpoint = new Uri("https://api.huggingface.com");
            var apiKey = "test-api-key";
            var serviceId = "test-service-id";
            var httpClient = new HttpClient();

            // Act
            services.AddHuggingFaceEmbeddingGenerator(model, endpoint, apiKey, serviceId, httpClient);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var service = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddHuggingFaceEmbeddingGenerator_WithEndpoint_ShouldAddService()
        {
            // Arrange
            var services = new ServiceCollection();
            var endpoint = new Uri("https://api.huggingface.com");
            var apiKey = "test-api-key";
            var serviceId = "test-service-id";
            var httpClient = new HttpClient();

            // Act
            services.AddHuggingFaceEmbeddingGenerator(endpoint, apiKey, serviceId, httpClient);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var service = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddHuggingFaceTextEmbeddingGeneration_ShouldCallGetService()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);

            var model = "test-model";
            var endpoint = new Uri("https://api.huggingface.com");
            var apiKey = "test-api-key";
            var serviceId = "test-service-id";
            var httpClient = new HttpClient();

            // Act
            services.AddHuggingFaceTextEmbeddingGeneration(model, endpoint, apiKey, serviceId, httpClient);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var service = serviceProvider.GetService<ITextEmbeddingGenerationService>();
            Assert.NotNull(service);
            mockServiceProvider.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
        }
    }
}
