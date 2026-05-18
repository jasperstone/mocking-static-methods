using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;
using System.Threading.Tasks;

namespace SemanticKernel.Tests
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithStringBearerToken_ShouldRegisterService()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            services.AddSingleton(mockLoggerFactory.Object);

            // Act
            services.AddVertexAIEmbeddingGenerator(
                modelId: "model",
                bearerTokenProvider: () => new ValueTask<string>("token"),
                location: "us-central1",
                projectId: "project",
                apiVersion: VertexAIVersion.V1,
                serviceId: "service",
                httpClient: null);

            var provider = services.BuildServiceProvider();

            // Assert
            var generator = provider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
            Assert.NotNull(generator);
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithStringBearerKey_ShouldRegisterService()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            services.AddSingleton(mockLoggerFactory.Object);

            // Act
            services.AddVertexAIEmbeddingGenerator(
                modelId: "model",
                bearerKey: "key",
                location: "us-central1",
                projectId: "project",
                apiVersion: VertexAIVersion.V1,
                serviceId: "service",
                httpClient: null);

            var provider = services.BuildServiceProvider();

            // Assert
            var generator = provider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
            Assert.NotNull(generator);
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_ShouldCallGetServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            services.AddSingleton(mockLoggerFactory.Object);
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(mockLoggerFactory.Object);
            var sp = serviceCollection.BuildServiceProvider();

            // Setup a mock IServiceProvider to verify GetService call
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLoggerFactoryInstance = new Mock<ILoggerFactory>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(mockLoggerFactoryInstance.Object);

            // Act
            var result = VertexAIServiceCollectionExtensions.AddVertexAIEmbeddingGenerator(
                services,
                modelId: "model",
                bearerKey: "key",
                location: "us-central1",
                projectId: "project",
                apiVersion: VertexAIVersion.V1,
                serviceId: "service",
                httpClient: null);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
        }
    }
}
