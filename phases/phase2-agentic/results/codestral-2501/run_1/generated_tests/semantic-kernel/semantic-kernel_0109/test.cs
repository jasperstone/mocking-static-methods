using System;
using System.Net.Http;
using System.Threading.Tasks;
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
        public void AddVertexAIEmbeddingGenerator_WithBearerTokenProvider_ShouldAddService()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "modelId";
            var bearerTokenProvider = new Func<ValueTask<string>>(() => new ValueTask<string>("token"));
            var location = "location";
            var projectId = "projectId";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "serviceId";
            var httpClient = new HttpClient();

            // Act
            services.AddVertexAIEmbeddingGenerator(modelId, bearerTokenProvider, location, projectId, apiVersion, serviceId, httpClient);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var embeddingGenerator = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>(serviceId);
            Assert.NotNull(embeddingGenerator);
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithBearerKey_ShouldAddService()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "modelId";
            var bearerKey = "bearerKey";
            var location = "location";
            var projectId = "projectId";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "serviceId";
            var httpClient = new HttpClient();

            // Act
            services.AddVertexAIEmbeddingGenerator(modelId, bearerKey, location, projectId, apiVersion, serviceId, httpClient);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var embeddingGenerator = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>(serviceId);
            Assert.NotNull(embeddingGenerator);
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_ShouldCallGetService()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "modelId";
            var bearerTokenProvider = new Func<ValueTask<string>>(() => new ValueTask<string>("token"));
            var location = "location";
            var projectId = "projectId";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "serviceId";
            var httpClient = new HttpClient();

            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);

            services.AddSingleton<IServiceProvider>(mockServiceProvider.Object);

            // Act
            services.AddVertexAIEmbeddingGenerator(modelId, bearerTokenProvider, location, projectId, apiVersion, serviceId, httpClient);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
        }
    }
}
