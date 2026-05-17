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
        public void AddVertexAIEmbeddingGenerator_ShouldRegisterService()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "test-model";
            var bearerTokenProvider = new Func<ValueTask<string>>(() => new ValueTask<string>("test-token"));
            var location = "test-location";
            var projectId = "test-project";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "test-service";
            var httpClient = new HttpClient();

            // Act
            services.AddVertexAIEmbeddingGenerator(modelId, bearerTokenProvider, location, projectId, apiVersion, serviceId, httpClient);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var embeddingGenerator = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
            Assert.NotNull(embeddingGenerator);
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithBearerKey_ShouldRegisterService()
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
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var embeddingGenerator = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
            Assert.NotNull(embeddingGenerator);
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_ShouldCallGetServiceForLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "test-model";
            var bearerTokenProvider = new Func<ValueTask<string>>(() => new ValueTask<string>("test-token"));
            var location = "test-location";
            var projectId = "test-project";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "test-service";
            var httpClient = new HttpClient();

            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);

            // Act
            services.AddVertexAIEmbeddingGenerator(modelId, bearerTokenProvider, location, projectId, apiVersion, serviceId, httpClient);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            mockServiceProvider.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
        }
    }
}
