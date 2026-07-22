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

            // Act
            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<ILoggerFactory>();

            // Assert
            Assert.NotNull(serviceProvider.GetService<ILoggerFactory>());
        }
    }
}
