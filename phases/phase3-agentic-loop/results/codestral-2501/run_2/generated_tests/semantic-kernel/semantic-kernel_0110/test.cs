using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Embeddings;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGeneration_WithBearerTokenProvider_ShouldRegisterService()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "modelId";
            var bearerTokenProvider = new Func<ValueTask<string>>(() => new ValueTask<string>("bearerToken"));
            var location = "location";
            var projectId = "projectId";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "serviceId";

            // Act
            services.AddVertexAIEmbeddingGeneration(modelId, bearerTokenProvider, location, projectId, apiVersion, serviceId);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var embeddingGenerator = serviceProvider.GetService<ITextEmbeddingGenerationService>();
            Assert.NotNull(embeddingGenerator);
        }

        [Fact]
        public void AddVertexAIEmbeddingGeneration_WithBearerKey_ShouldRegisterService()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "modelId";
            var bearerKey = "bearerKey";
            var location = "location";
            var projectId = "projectId";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "serviceId";

            // Act
            services.AddVertexAIEmbeddingGeneration(modelId, bearerKey, location, projectId, apiVersion, serviceId);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var embeddingGenerator = serviceProvider.GetService<ITextEmbeddingGenerationService>();
            Assert.NotNull(embeddingGenerator);
        }

        [Fact]
        public void AddVertexAIEmbeddingGeneration_ShouldCallGetServiceForLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "modelId";
            var bearerKey = "bearerKey";
            var location = "location";
            var projectId = "projectId";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "serviceId";

            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);

            services.AddSingleton<IServiceProvider>(mockServiceProvider.Object);

            // Act
            services.AddVertexAIEmbeddingGeneration(modelId, bearerKey, location, projectId, apiVersion, serviceId);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            mockServiceProvider.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
        }
    }
}
