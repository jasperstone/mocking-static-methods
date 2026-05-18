using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.Extensions.Logging;
using Moq;
using System;

namespace Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHuggingFaceEmbeddingGenerator_ShouldRegisterService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var model = "test-model";
            var endpoint = new Uri("https://api.huggingface.com");
            var apiKey = "test-api-key";
            var serviceId = "test-service-id";

            // Act
            serviceCollection.AddHuggingFaceEmbeddingGenerator(model, endpoint, apiKey, serviceId);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var service = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddHuggingFaceEmbeddingGenerator_ShouldRegisterServiceWithLogger()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var model = "test-model";
            var endpoint = new Uri("https://api.huggingface.com");
            var apiKey = "test-api-key";
            var serviceId = "test-service-id";
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            serviceCollection.AddSingleton(mockLoggerFactory.Object);

            // Act
            serviceCollection.AddHuggingFaceEmbeddingGenerator(model, endpoint, apiKey, serviceId);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var service = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
            Assert.NotNull(service);
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            Assert.NotNull(loggerFactory);
        }
    }
}
