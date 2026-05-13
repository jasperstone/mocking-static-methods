using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHuggingFaceTextEmbeddingGeneration_GetServiceLoggerFactory_ReturnsLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            // Assert
            Assert.NotNull(loggerFactory);
        }

        [Fact]
        public void AddHuggingFaceImageToText_GetServiceLoggerFactory_ReturnsLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            // Assert
            Assert.NotNull(loggerFactory);
        }
    }
}
