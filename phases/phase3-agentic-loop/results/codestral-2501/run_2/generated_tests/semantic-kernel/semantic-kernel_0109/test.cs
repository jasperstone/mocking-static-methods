using System;
using System.Net.Http;
using System.Threading.Tasks;
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
        public void AddVertexAIEmbeddingGenerator_WithBearerTokenProvider_ShouldAddService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var modelId = "testModelId";
            var bearerTokenProvider = new Func<ValueTask<string>>(() => new ValueTask<string>("testToken"));
            var location = "testLocation";
            var projectId = "testProjectId";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "testServiceId";
            var httpClient = new HttpClient();

            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);

            serviceCollection.AddSingleton<ILoggerFactory>(mockLoggerFactory.Object);

            // Act
            var result = VertexAIServiceCollectionExtensions.AddVertexAIEmbeddingGenerator(
                serviceCollection,
                modelId,
                bearerTokenProvider,
                location,
                projectId,
                apiVersion,
                serviceId,
                httpClient);

            // Assert
            var serviceProvider = result.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            Assert.NotNull(loggerFactory);
            Assert.Same(mockLoggerFactory.Object, loggerFactory);
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithBearerKey_ShouldAddService()
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

            serviceCollection.AddSingleton<ILoggerFactory>(mockLoggerFactory.Object);

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
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            Assert.NotNull(loggerFactory);
            Assert.Same(mockLoggerFactory.Object, loggerFactory);
        }
    }
}
