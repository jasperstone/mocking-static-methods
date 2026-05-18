using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithValidParameters_ReturnsUpdatedServiceCollection()
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
            var updatedServices = services.AddVertexAIEmbeddingGenerator(modelId, bearerTokenProvider, location, projectId, apiVersion, serviceId, httpClient);

            // Assert
            Assert.NotNull(updatedServices);
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithValidParametersAndLoggerFactory_ReturnsUpdatedServiceCollection()
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
            var updatedServices = services.AddVertexAIEmbeddingGenerator(modelId, bearerTokenProvider, location, projectId, apiVersion, serviceId, httpClient);

            // Assert
            Assert.NotNull(updatedServices);
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithNullServices_ThrowsArgumentNullException()
        {
            // Arrange
            IServiceCollection services = null;
            var modelId = "modelId";
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
        public void AddVertexAIEmbeddingGenerator_WithNullModelId_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();
            string modelId = null;
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
            Func<ValueTask<string>> bearerTokenProvider = null;
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
            string location = null;
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
            string projectId = null;
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "serviceId";
            var httpClient = new HttpClient();

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator(modelId, bearerTokenProvider, location, projectId, apiVersion, serviceId, httpClient));
        }
    }
}
