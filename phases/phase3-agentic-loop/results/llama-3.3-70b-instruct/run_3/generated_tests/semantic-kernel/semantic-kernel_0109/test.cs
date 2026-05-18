using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Google;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithValidParameters_ReturnsServiceCollection()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "modelId";
            var bearerTokenProvider = () => ValueTask.FromResult("token");
            var location = "location";
            var projectId = "projectId";
            var apiVersion = VertexAIVersion.V1;

            // Act
            var result = services.AddVertexAIEmbeddingGenerator(modelId, bearerTokenProvider, location, projectId, apiVersion);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithNullServices_ThrowsArgumentNullException()
        {
            // Arrange
            IServiceCollection services = null;
            var modelId = "modelId";
            var bearerTokenProvider = () => ValueTask.FromResult("token");
            var location = "location";
            var projectId = "projectId";
            var apiVersion = VertexAIVersion.V1;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator(modelId, bearerTokenProvider, location, projectId, apiVersion));
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithNullModelId_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();
            string modelId = null;
            var bearerTokenProvider = () => ValueTask.FromResult("token");
            var location = "location";
            var projectId = "projectId";
            var apiVersion = VertexAIVersion.V1;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator(modelId, bearerTokenProvider, location, projectId, apiVersion));
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

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator(modelId, bearerTokenProvider, location, projectId, apiVersion));
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithNullLocation_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "modelId";
            var bearerTokenProvider = () => ValueTask.FromResult("token");
            string location = null;
            var projectId = "projectId";
            var apiVersion = VertexAIVersion.V1;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator(modelId, bearerTokenProvider, location, projectId, apiVersion));
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithNullProjectId_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "modelId";
            var bearerTokenProvider = () => ValueTask.FromResult("token");
            var location = "location";
            string projectId = null;
            var apiVersion = VertexAIVersion.V1;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator(modelId, bearerTokenProvider, location, projectId, apiVersion));
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_ServiceProviderGetService_ReturnsLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            var serviceProvider = services.BuildServiceProvider();
            var modelId = "modelId";
            var bearerTokenProvider = () => ValueTask.FromResult("token");
            var location = "location";
            var projectId = "projectId";
            var apiVersion = VertexAIVersion.V1;
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            // Act
            services.AddVertexAIEmbeddingGenerator(modelId, bearerTokenProvider, location, projectId, apiVersion);

            // Assert
            Assert.NotNull(loggerFactory);
        }
    }
}
