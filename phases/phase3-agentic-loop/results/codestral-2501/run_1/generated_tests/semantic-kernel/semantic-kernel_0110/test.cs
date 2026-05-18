using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Embeddings;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGenerator_ShouldAddServiceToCollection()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "test-model";
            var bearerKey = "test-key";
            var location = "test-location";
            var projectId = "test-project";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "test-service";

            // Act
            services.AddVertexAIEmbeddingGenerator(modelId, bearerKey, location, projectId, apiVersion, serviceId);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var embeddingGenerator = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>(serviceId);
            Assert.NotNull(embeddingGenerator);
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_ShouldUseCustomHttpClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "test-model";
            var bearerKey = "test-key";
            var location = "test-location";
            var projectId = "test-project";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "test-service";
            var httpClient = new HttpClient();

            // Act
            services.AddVertexAIEmbeddingGenerator(modelId, bearerKey, location, projectId, apiVersion, serviceId, httpClient);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var embeddingGenerator = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>(serviceId);
            Assert.NotNull(embeddingGenerator);
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_ShouldGetLoggerFactoryFromServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "test-model";
            var bearerKey = "test-key";
            var location = "test-location";
            var projectId = "test-project";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "test-service";
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            services.AddSingleton(mockLoggerFactory.Object);

            // Act
            services.AddVertexAIEmbeddingGenerator(modelId, bearerKey, location, projectId, apiVersion, serviceId);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var embeddingGenerator = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>(serviceId);
            Assert.NotNull(embeddingGenerator);
            mockLoggerFactory.Verify(f => f.CreateLogger(It.IsAny<string>()), Times.Once);
        }
    }
}
