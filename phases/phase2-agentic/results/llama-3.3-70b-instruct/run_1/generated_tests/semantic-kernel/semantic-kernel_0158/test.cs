using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaTextEmbeddingGeneration_ServiceProvider_GetService_OllamaApiClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var ollamaClient = new OllamaApiClient(new HttpClient(), "modelId");

            services.AddSingleton(ollamaClient);

            // Act
            var result = services.AddOllamaTextEmbeddingGeneration(ollamaClient: null);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void AddOllamaTextEmbeddingGeneration_ServiceProvider_GetService_OllamaApiClient_Null()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => services.AddOllamaTextEmbeddingGeneration(ollamaClient: null));
        }

        [Fact]
        public void AddOllamaTextEmbeddingGeneration_ServiceProvider_GetKeyedService_OllamaApiClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var ollamaClient = new OllamaApiClient(new HttpClient(), "modelId");

            services.AddKeyedSingleton<OllamaApiClient>("serviceId", ollamaClient);

            // Act
            var result = services.AddOllamaTextEmbeddingGeneration(ollamaClient: null, serviceId: "serviceId");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void AddOllamaTextEmbeddingGeneration_ServiceProvider_GetKeyedService_IOllamaApiClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var ollamaClient = new OllamaApiClient(new HttpClient(), "modelId");

            services.AddKeyedSingleton<IOllamaApiClient>("serviceId", ollamaClient);

            // Act
            var result = services.AddOllamaTextEmbeddingGeneration(ollamaClient: null, serviceId: "serviceId");

            // Assert
            Assert.NotNull(result);
        }
    }
}
