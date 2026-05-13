using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;
using Moq;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Http;
using Microsoft.Extensions.AI;

namespace Connectors.Google.Tests
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithNullServices_Throws()
        {
            // Arrange
            IServiceCollection services = null;
            var modelId = "model";
            var bearerKey = "key";
            var location = "us-central1";
            var projectId = "project";

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                VertexAIServiceCollectionExtensions.AddVertexAIEmbeddingGenerator(
                    services,
                    modelId,
                    bearerKey,
                    location,
                    projectId));
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithNullModelId_Throws()
        {
            var services = new ServiceCollection();
            var mockServices = services;
            var modelId = (string)null;
            var bearerKey = "key";
            var location = "us-central1";
            var projectId = "project";

            Assert.Throws<ArgumentNullException>(() =>
                VertexAIServiceCollectionExtensions.AddVertexAIEmbeddingGenerator(
                    mockServices,
                    modelId,
                    bearerKey,
                    location,
                    projectId));
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithNullBearerKey_Throws()
        {
            var services = new ServiceCollection();
            var modelId = "model";
            string bearerKey = null;
            var location = "us-central1";
            var projectId = "project";

            Assert.Throws<ArgumentNullException>(() =>
                VertexAIServiceCollectionExtensions.AddVertexAIEmbeddingGenerator(
                    services,
                    modelId,
                    bearerKey,
                    location,
                    projectId));
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithNullLocation_Throws()
        {
            var services = new ServiceCollection();
            var modelId = "model";
            var bearerKey = "key";
            string location = null;
            var projectId = "project";

            Assert.Throws<ArgumentNullException>(() =>
                VertexAIServiceCollectionExtensions.AddVertexAIEmbeddingGenerator(
                    services,
                    modelId,
                    bearerKey,
                    location,
                    projectId));
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithNullProjectId_Throws()
        {
            var services = new ServiceCollection();
            var modelId = "model";
            var bearerKey = "key";
            var location = "us-central1";
            string projectId = null;

            Assert.Throws<ArgumentNullException>(() =>
                VertexAIServiceCollectionExtensions.AddVertexAIEmbeddingGenerator(
                    services,
                    modelId,
                    bearerKey,
                    location,
                    projectId));
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithValidParameters_ReturnsServiceCollection()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "model";
            var bearerKey = "key";
            var location = "us-central1";
            var projectId = "project";

            // Add a mock ILoggerFactory to the service provider
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            services.AddSingleton(loggerFactoryMock.Object);

            // Act
            var result = services.AddVertexAIEmbeddingGenerator(
                modelId,
                bearerKey,
                location,
                projectId);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ServiceCollection>(result);
            var provider = result.BuildServiceProvider();

            // Verify that the service provider can resolve ILoggerFactory
            var loggerFactory = provider.GetService<ILoggerFactory>();
            Assert.NotNull(loggerFactory);
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithHttpClientAndServiceProvider_GetServiceCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "model";
            var bearerKey = "key";
            var location = "us-central1";
            var projectId = "project";

            // Add a mock ILoggerFactory to the service provider
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            services.AddSingleton(loggerFactoryMock.Object);

            // Add a mock HttpClient
            var mockHttpClient = new HttpClient();

            // Build the service provider
            var provider = services.BuildServiceProvider();

            // Act
            var result = services.AddVertexAIEmbeddingGenerator(
                modelId,
                bearerKey,
                location,
                projectId,
                httpClient: mockHttpClient);

            // Assert
            Assert.NotNull(result);
            var sp = result.BuildServiceProvider();

            // Verify that GetService<ILoggerFactory> is called
            var loggerFactory = sp.GetService<ILoggerFactory>();
            Assert.NotNull(loggerFactory);
        }
    }
}
