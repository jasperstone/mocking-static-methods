using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

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
            var bearerKey = "bearerKey";
            var location = "location";
            var projectId = "projectId";

            // Act
            var result = services.AddVertexAIEmbeddingGenerator(modelId, bearerKey, location, projectId);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithNullModelId_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();
            string? modelId = null;
            var bearerKey = "bearerKey";
            var location = "location";
            var projectId = "projectId";

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator(modelId, bearerKey, location, projectId));
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithNullBearerKey_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "modelId";
            string? bearerKey = null;
            var location = "location";
            var projectId = "projectId";

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator(modelId, bearerKey, location, projectId));
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithNullLocation_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "modelId";
            var bearerKey = "bearerKey";
            string? location = null;
            var projectId = "projectId";

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator(modelId, bearerKey, location, projectId));
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithNullProjectId_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "modelId";
            var bearerKey = "bearerKey";
            var location = "location";
            string? projectId = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator(modelId, bearerKey, location, projectId));
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_GetService_ReturnsLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());
            var modelId = "modelId";
            var bearerKey = "bearerKey";
            var location = "location";
            var projectId = "projectId";

            // Act
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            // Assert
            Assert.NotNull(loggerFactory);
        }
    }
}
