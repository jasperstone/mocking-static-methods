using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Google;

namespace VertexAIServiceCollectionExtensionsTests
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGenerator_ServiceProvider_GetService_ReturnsLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            // Act
            var result = services.AddVertexAIEmbeddingGenerator(
                "modelId",
                "bearerKey",
                "location",
                "projectId",
                VertexAIVersion.V1,
                null,
                null);

            // Assert
            var serviceProviderResult = result.BuildServiceProvider();
            var loggerFactoryResult = serviceProviderResult.GetService<ILoggerFactory>();
            Assert.NotNull(loggerFactoryResult);
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_ServiceProvider_GetService_ThrowsException_WhenLoggerFactoryIsNull()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => services.AddVertexAIEmbeddingGenerator(
                "modelId",
                "bearerKey",
                "location",
                "projectId",
                VertexAIVersion.V1,
                null,
                null));
        }
    }
}
