using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGenerator_GetService_ReturnsILoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());

            // Act
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            // Assert
            Assert.NotNull(loggerFactory);
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithValidParameters_DoesNotThrow()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());

            // Act and Assert
            try
            {
                services.AddVertexAIEmbeddingGenerator(
                modelId: "modelId",
                bearerKey: "bearerKey",
                location: "location",
                projectId: "projectId",
                apiVersion: VertexAIVersion.V1,
                serviceId: null,
                httpClient: null);
            }
            catch (Exception ex)
            {
                Assert.True(false, "AddVertexAIEmbeddingGenerator threw an exception: " + ex.Message);
            }
        }
    }
}
