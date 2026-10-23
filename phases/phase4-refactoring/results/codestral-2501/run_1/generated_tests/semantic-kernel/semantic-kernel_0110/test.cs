using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Google;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGenerator_ShouldAddServiceToCollection()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var modelId = "modelId";
            var bearerKey = "bearerKey";
            var location = "location";
            var projectId = "projectId";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "serviceId";
            var httpClient = new HttpClient();

            // Act
            serviceCollection.AddVertexAIEmbeddingGenerator(modelId, bearerKey, location, projectId, apiVersion, serviceId, httpClient);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var embeddingGenerator = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
            Assert.NotNull(embeddingGenerator);
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_ShouldAddServiceToCollection_WithBearerTokenProvider()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var modelId = "modelId";
            var bearerTokenProvider = new Func<ValueTask<string>>(() => new ValueTask<string>("bearerToken"));
            var location = "location";
            var projectId = "projectId";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "serviceId";
            var httpClient = new HttpClient();

            // Act
            serviceCollection.AddVertexAIEmbeddingGenerator(modelId, bearerTokenProvider, location, projectId, apiVersion, serviceId, httpClient);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var embeddingGenerator = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
            Assert.NotNull(embeddingGenerator);
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_ShouldAddLoggerFactory()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var modelId = "modelId";
            var bearerKey = "bearerKey";
            var location = "location";
            var projectId = "projectId";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "serviceId";
            var httpClient = new HttpClient();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceCollection.AddSingleton(loggerFactoryMock.Object);

            // Act
            serviceCollection.AddVertexAIEmbeddingGenerator(modelId, bearerKey, location, projectId, apiVersion, serviceId, httpClient);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            Assert.NotNull(loggerFactory);
            Assert.Same(loggerFactoryMock.Object, loggerFactory);
        }
    }
}
