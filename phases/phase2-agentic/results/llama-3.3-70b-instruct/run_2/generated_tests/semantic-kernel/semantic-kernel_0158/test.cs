using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using OllamaSharp;

namespace TestProject
{
    [TestClass]
    public class OllamaServiceCollectionExtensionsTests
    {
        [TestMethod]
        public void AddOllamaTextEmbeddingGeneration_ServiceProvider_GetService_OllamaApiClient_ReturnsOllamaApiClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var ollamaApiClient = new OllamaApiClient(new Uri("https://example.com"), "modelId");
            services.AddSingleton(ollamaApiClient);

            // Act
            var result = OllamaServiceCollectionExtensions.AddOllamaTextEmbeddingGeneration(services, ollamaApiClient);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void AddOllamaTextEmbeddingGeneration_ServiceProvider_GetService_OllamaApiClient_ThrowsInvalidOperationException_WhenNoOllamaApiClientFound()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            // Act and Assert
            Assert.ThrowsException<InvalidOperationException>(() => OllamaServiceCollectionExtensions.AddOllamaTextEmbeddingGeneration(services));
        }

        [TestMethod]
        public void AddOllamaTextEmbeddingGeneration_ServiceProvider_GetService_OllamaApiClient_ReturnsOllamaApiClient_WhenOllamaApiClientIsRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var ollamaApiClient = new OllamaApiClient(new Uri("https://example.com"), "modelId");
            services.AddSingleton(ollamaApiClient);
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = OllamaServiceCollectionExtensions.AddOllamaTextEmbeddingGeneration(services, ollamaApiClient);

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
