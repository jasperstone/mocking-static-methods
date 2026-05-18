using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Connectors.Google;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithBearerTokenProvider_CallsGetService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();

            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(mockLoggerFactory.Object);

            serviceCollection.AddSingleton<IServiceProvider>(mockServiceProvider.Object);

            string modelId = "testModelId";
            Func<ValueTask<string>> bearerTokenProvider = async () => "testToken";
            string location = "testLocation";
            string projectId = "testProjectId";

            // Act
            serviceCollection.AddVertexAIEmbeddingGenerator(modelId, bearerTokenProvider, location, projectId);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithBearerKey_CallsGetService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();

            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(mockLoggerFactory.Object);

            serviceCollection.AddSingleton<IServiceProvider>(mockServiceProvider.Object);

            string modelId = "testModelId";
            string bearerKey = "testBearerKey";
            string location = "testLocation";
            string projectId = "testProjectId";

            // Act
            serviceCollection.AddVertexAIEmbeddingGenerator(modelId, bearerKey, location, projectId);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
        }
    }
}
