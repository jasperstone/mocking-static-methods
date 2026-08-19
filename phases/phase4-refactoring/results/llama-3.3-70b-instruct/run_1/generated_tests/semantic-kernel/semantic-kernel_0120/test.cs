using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net.Http;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHuggingFaceTextEmbeddingGeneration_ValidInput_ReturnsSameInstance()
        {
            // Arrange
            var services = new ServiceCollection();
            var endpoint = new Uri("https://example.com");
            var apiKey = "api-key";
            var serviceId = "service-id";
            var httpClient = new HttpClient();

            // Act
            var result = HuggingFaceServiceCollectionExtensions.AddHuggingFaceTextEmbeddingGeneration(services, endpoint, apiKey, serviceId, httpClient);

            // Assert
            Assert.Same(services, result);
        }

        [Fact]
        public void AddHuggingFaceTextEmbeddingGeneration_NullServices_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => HuggingFaceServiceCollectionExtensions.AddHuggingFaceTextEmbeddingGeneration(null, new Uri("https://example.com"), "api-key", "service-id", new HttpClient()));
        }

        [Fact]
        public void AddHuggingFaceTextEmbeddingGeneration_GetService_ReturnsLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());
            var endpoint = new Uri("https://example.com");
            var apiKey = "api-key";
            var serviceId = "service-id";
            var httpClient = new HttpClient();
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            // Act
            var result = HuggingFaceServiceCollectionExtensions.AddHuggingFaceTextEmbeddingGeneration(services, endpoint, apiKey, serviceId, httpClient);

            // Assert
            Assert.NotNull(loggerFactory);
        }
    }
}
