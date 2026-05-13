using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHuggingFaceTextEmbeddingGeneration_ServiceProvider_GetService_Called()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            // Act
            services.AddHuggingFaceTextEmbeddingGeneration(new Uri("https://example.com"), "apiKey", "serviceId", new HttpClient());

            // Assert
            Assert.NotNull(loggerFactory);
        }

        [Fact]
        public void AddHuggingFaceImageToText_ServiceProvider_GetService_Called()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            // Act
            services.AddHuggingFaceImageToText("model", new Uri("https://example.com"), "apiKey", "serviceId", new HttpClient());

            // Assert
            Assert.NotNull(loggerFactory);
        }
    }
}
