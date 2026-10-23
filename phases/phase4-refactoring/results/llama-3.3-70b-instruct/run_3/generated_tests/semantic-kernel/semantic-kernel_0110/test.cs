using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithValidParameters_ReturnsUpdatedServiceCollection()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "modelId";
            var bearerKey = "bearerKey";
            var location = "location";
            var projectId = "projectId";
            var apiVersion = Microsoft.SemanticKernel.Connectors.Google.VertexAIVersion.V1;
            var serviceId = "serviceId";
            var httpClient = new HttpClient();

            // Act
            var updatedServices = VertexAIServiceCollectionExtensions.AddVertexAIEmbeddingGenerator(
                services,
                modelId,
                bearerKey,
                location,
                projectId,
                apiVersion,
                serviceId,
                httpClient);

            // Assert
            Assert.NotNull(updatedServices);
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithNullModelId_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();
            string modelId = null;
            var bearerKey = "bearerKey";
            var location = "location";
            var projectId = "projectId";
            var apiVersion = Microsoft.SemanticKernel.Connectors.Google.VertexAIVersion.V1;
            var serviceId = "serviceId";
            var httpClient = new HttpClient();

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => VertexAIServiceCollectionExtensions.AddVertexAIEmbeddingGenerator(
                services,
                modelId,
                bearerKey,
                location,
                projectId,
                apiVersion,
                serviceId,
                httpClient));
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithNullBearerKey_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "modelId";
            string bearerKey = null;
            var location = "location";
            var projectId = "projectId";
            var apiVersion = Microsoft.SemanticKernel.Connectors.Google.VertexAIVersion.V1;
            var serviceId = "serviceId";
            var httpClient = new HttpClient();

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => VertexAIServiceCollectionExtensions.AddVertexAIEmbeddingGenerator(
                services,
                modelId,
                bearerKey,
                location,
                projectId,
                apiVersion,
                serviceId,
                httpClient));
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithNullLocation_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "modelId";
            var bearerKey = "bearerKey";
            string location = null;
            var projectId = "projectId";
            var apiVersion = Microsoft.SemanticKernel.Connectors.Google.VertexAIVersion.V1;
            var serviceId = "serviceId";
            var httpClient = new HttpClient();

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => VertexAIServiceCollectionExtensions.AddVertexAIEmbeddingGenerator(
                services,
                modelId,
                bearerKey,
                location,
                projectId,
                apiVersion,
                serviceId,
                httpClient));
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithNullProjectId_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "modelId";
            var bearerKey = "bearerKey";
            var location = "location";
            string projectId = null;
            var apiVersion = Microsoft.SemanticKernel.Connectors.Google.VertexAIVersion.V1;
            var serviceId = "serviceId";
            var httpClient = new HttpClient();

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => VertexAIServiceCollectionExtensions.AddVertexAIEmbeddingGenerator(
                services,
                modelId,
                bearerKey,
                location,
                projectId,
                apiVersion,
                serviceId,
                httpClient));
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithNullServiceId_DoesNotThrow()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "modelId";
            var bearerKey = "bearerKey";
            var location = "location";
            var projectId = "projectId";
            var apiVersion = Microsoft.SemanticKernel.Connectors.Google.VertexAIVersion.V1;
            string serviceId = null;
            var httpClient = new HttpClient();

            // Act
            var updatedServices = VertexAIServiceCollectionExtensions.AddVertexAIEmbeddingGenerator(
                services,
                modelId,
                bearerKey,
                location,
                projectId,
                apiVersion,
                serviceId,
                httpClient);

            // Assert
            Assert.NotNull(updatedServices);
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithNullHttpClient_DoesNotThrow()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "modelId";
            var bearerKey = "bearerKey";
            var location = "location";
            var projectId = "projectId";
            var apiVersion = Microsoft.SemanticKernel.Connectors.Google.VertexAIVersion.V1;
            var serviceId = "serviceId";
            HttpClient httpClient = null;

            // Act
            var updatedServices = VertexAIServiceCollectionExtensions.AddVertexAIEmbeddingGenerator(
                services,
                modelId,
                bearerKey,
                location,
                projectId,
                apiVersion,
                serviceId,
                httpClient);

            // Assert
            Assert.NotNull(updatedServices);
        }
    }
}
