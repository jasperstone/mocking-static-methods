using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Google;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithValidParameters_AddsServiceToCollection()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "modelId";
            var bearerTokenProvider = () => new ValueTask<string>("bearerToken");
            var location = "location";
            var projectId = "projectId";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "serviceId";
            var httpClient = new HttpClient();

            // Act
            services.AddVertexAIEmbeddingGenerator(modelId, bearerTokenProvider, location, projectId, apiVersion, serviceId, httpClient);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var embeddingGenerator = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
            Assert.NotNull(embeddingGenerator);
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithValidParametersAndLoggerFactory_AddsServiceToCollection()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "modelId";
            var bearerTokenProvider = () => new ValueTask<string>("bearerToken");
            var location = "location";
            var projectId = "projectId";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "serviceId";
            var httpClient = new HttpClient();
            var loggerFactory = new LoggerFactory();

            // Act
            services.AddVertexAIEmbeddingGenerator(modelId, bearerTokenProvider, location, projectId, apiVersion, serviceId, httpClient);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var embeddingGenerator = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
            Assert.NotNull(embeddingGenerator);
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithNullModelId_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();
            string? modelId = null;
            var bearerTokenProvider = () => new ValueTask<string>("bearerToken");
            var location = "location";
            var projectId = "projectId";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "serviceId";
            var httpClient = new HttpClient();

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator(modelId, bearerTokenProvider, location, projectId, apiVersion, serviceId, httpClient));
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithNullBearerTokenProvider_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "modelId";
            Func<ValueTask<string>>? bearerTokenProvider = null;
            var location = "location";
            var projectId = "projectId";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "serviceId";
            var httpClient = new HttpClient();

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator(modelId, bearerTokenProvider, location, projectId, apiVersion, serviceId, httpClient));
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithNullLocation_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "modelId";
            var bearerTokenProvider = () => new ValueTask<string>("bearerToken");
            string? location = null;
            var projectId = "projectId";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "serviceId";
            var httpClient = new HttpClient();

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator(modelId, bearerTokenProvider, location, projectId, apiVersion, serviceId, httpClient));
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithNullProjectId_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "modelId";
            var bearerTokenProvider = () => new ValueTask<string>("bearerToken");
            var location = "location";
            string? projectId = null;
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "serviceId";
            var httpClient = new HttpClient();

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator(modelId, bearerTokenProvider, location, projectId, apiVersion, serviceId, httpClient));
        }
    }
}
