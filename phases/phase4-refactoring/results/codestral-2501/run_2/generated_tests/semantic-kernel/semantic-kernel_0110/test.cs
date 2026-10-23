using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Connectors.Google.Core;
using System.Net.Http;
using Moq;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGenerator_ShouldAddService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var modelId = "modelId";
            var bearerKey = "bearerKey";
            var location = "location";
            var projectId = "projectId";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "serviceId";
            var httpClient = new HttpClient();

            // Act
            serviceCollection.AddVertexAIEmbeddingGenerator(modelId, bearerKey, location, projectId, apiVersion, serviceId, httpClient);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var embeddingClient = serviceProvider.GetService<VertexAIEmbeddingClient>();
            Assert.NotNull(embeddingClient);
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_ShouldAddServiceWithDefaultParameters()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var modelId = "modelId";
            var bearerKey = "bearerKey";
            var location = "location";
            var projectId = "projectId";

            // Act
            serviceCollection.AddVertexAIEmbeddingGenerator(modelId, bearerKey, location, projectId);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var embeddingClient = serviceProvider.GetService<VertexAIEmbeddingClient>();
            Assert.NotNull(embeddingClient);
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_ShouldAddServiceWithBearerTokenProvider()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var modelId = "modelId";
            var bearerTokenProvider = new Func<ValueTask<string>>(() => new ValueTask<string>("bearerToken"));
            var location = "location";
            var projectId = "projectId";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "serviceId";
            var httpClient = new HttpClient();

            // Act
            serviceCollection.AddVertexAIEmbeddingGenerator(modelId, bearerTokenProvider, location, projectId, apiVersion, serviceId, httpClient);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var embeddingClient = serviceProvider.GetService<VertexAIEmbeddingClient>();
            Assert.NotNull(embeddingClient);
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_ShouldAddServiceWithBearerTokenProviderAndDefaultParameters()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var modelId = "modelId";
            var bearerTokenProvider = new Func<ValueTask<string>>(() => new ValueTask<string>("bearerToken"));
            var location = "location";
            var projectId = "projectId";

            // Act
            serviceCollection.AddVertexAIEmbeddingGenerator(modelId, bearerTokenProvider, location, projectId);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var embeddingClient = serviceProvider.GetService<VertexAIEmbeddingClient>();
            Assert.NotNull(embeddingClient);
        }
    }
}
