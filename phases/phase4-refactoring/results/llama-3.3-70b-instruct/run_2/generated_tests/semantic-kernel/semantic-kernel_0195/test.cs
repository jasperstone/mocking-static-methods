using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOpenAITextEmbeddingGeneration_ServiceProviderGetService_ReturnsLoggerFactory()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ILoggerFactory>(Mock.Of<ILoggerFactory>())
                .BuildServiceProvider();

            var services = new ServiceCollection();
            services.AddOpenAITextEmbeddingGeneration("modelId", null, null, null);

            // Act
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            // Assert
            Assert.NotNull(loggerFactory);
        }

        [Fact]
        public void AddOpenAITextEmbeddingGeneration_ServiceProviderGetService_DoesNotThrowException_WhenLoggerFactoryIsNull()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .BuildServiceProvider();

            var services = new ServiceCollection();
            services.AddOpenAITextEmbeddingGeneration("modelId", null, null, null);

            // Act and Assert
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            Assert.Null(loggerFactory);
        }
    }
}
