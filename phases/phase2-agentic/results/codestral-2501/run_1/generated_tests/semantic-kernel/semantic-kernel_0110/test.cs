using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGenerator_Throws_WhenServicesIsNull()
        {
            // Arrange
            IServiceCollection services = null;
            string modelId = "modelId";
            string bearerKey = "bearerKey";
            string location = "location";
            string projectId = "projectId";

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator(modelId, bearerKey, location, projectId));
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_Throws_WhenModelIdIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            string modelId = null;
            string bearerKey = "bearerKey";
            string location = "location";
            string projectId = "projectId";

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator(modelId, bearerKey, location, projectId));
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_Throws_WhenBearerKeyIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            string modelId = "modelId";
            string bearerKey = null;
            string location = "location";
            string projectId = "projectId";

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator(modelId, bearerKey, location, projectId));
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_Throws_WhenLocationIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            string modelId = "modelId";
            string bearerKey = "bearerKey";
            string location = null;
            string projectId = "projectId";

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator(modelId, bearerKey, location, projectId));
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_Throws_WhenProjectIdIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            string modelId = "modelId";
            string bearerKey = "bearerKey";
            string location = "location";
            string projectId = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator(modelId, bearerKey, location, projectId));
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_AddsServiceToCollection()
        {
            // Arrange
            var services = new ServiceCollection();
            string modelId = "modelId";
            string bearerKey = "bearerKey";
            string location = "location";
            string projectId = "projectId";
            var mockLoggerFactory = new Mock<ILoggerFactory>();

            // Act
            services.AddVertexAIEmbeddingGenerator(modelId, bearerKey, location, projectId);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var embeddingGenerator = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
            Assert.NotNull(embeddingGenerator);
        }
    }
}
