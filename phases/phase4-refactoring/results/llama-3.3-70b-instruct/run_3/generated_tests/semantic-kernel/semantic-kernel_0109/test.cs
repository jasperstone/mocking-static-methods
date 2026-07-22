using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Google;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGenerator_GetService_Called()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            // Act
            services.AddVertexAIEmbeddingGenerator(
                "modelId",
                () => ValueTask.FromResult("bearerToken"),
                "location",
                "projectId",
                VertexAIVersion.V1,
                null,
                null);

            // Assert
            Assert.NotNull(loggerFactory);
        }
    }
}
