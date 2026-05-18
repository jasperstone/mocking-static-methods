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
            var serviceCollection = new ServiceCollection();
            var modelId = "testModelId";
            var bearerKey = "testBearerKey";
            var location = "testLocation";
            var projectId = "testProjectId";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "testServiceId";
            var httpClient = new HttpClient();

            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);

            serviceCollection.AddSingleton(mockServiceProvider.Object);

            // Act
            var result = VertexAIServiceCollectionExtensions.AddVertexAIEmbeddingGenerator(
                serviceCollection,
                modelId,
                bearerKey,
                location,
                projectId,
                apiVersion,
                serviceId,
                httpClient);

            // Assert
            var serviceProvider = result.BuildServiceProvider();
            var embeddingGenerator = serviceProvider.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();

            Assert.NotNull(embeddingGenerator);
            Assert.IsType<VertexAIEmbeddingGenerator>(embeddingGenerator);
        }
    }
}
