using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOpenAITextEmbeddingGeneration_ShouldRetrieveLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var loggerFactory = new LoggerFactory();
            mockServiceProvider
                .Setup(sp => sp.GetService<ILoggerFactory>())
                .Returns(loggerFactory);

            var modelId = "text-embedding-3";
            var serviceId = "testServiceId";
            var dimensions = 512;

            // Act
            services.AddOpenAITextEmbeddingGeneration(modelId, null, serviceId, dimensions);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetService<ILoggerFactory>(), Times.Once);
        }
    }
}
