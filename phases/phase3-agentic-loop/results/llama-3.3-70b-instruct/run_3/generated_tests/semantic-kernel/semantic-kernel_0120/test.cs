using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Moq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public async Task AddHuggingFaceTextEmbeddingGeneration_ServiceProvider_GetService_ReturnsLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            var serviceProvider = services.BuildServiceProvider();

            var endpoint = new Uri("https://example.com");
            var apiKey = "api-key";
            var serviceId = "service-id";
            var httpClient = new HttpClient();

            // Act
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            // Assert
            Assert.NotNull(loggerFactory);
        }

        [Fact]
        public async Task AddHuggingFaceTextEmbeddingGeneration_ServiceProvider_GetService_ReturnsNull_WhenNoLoggerFactoryRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            var endpoint = new Uri("https://example.com");
            var apiKey = "api-key";
            var serviceId = "service-id";
            var httpClient = new HttpClient();

            // Act
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            // Assert
            Assert.Null(loggerFactory);
        }

        [Fact]
        public async Task AddHuggingFaceTextEmbeddingGeneration_ServiceProvider_GetService_ThrowsException_WhenNoServiceProvider()
        {
            // Arrange
            var endpoint = new Uri("https://example.com");
            var apiKey = "api-key";
            var serviceId = "service-id";
            var httpClient = new HttpClient();

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => HuggingFaceServiceCollectionExtensions.AddHuggingFaceTextEmbeddingGeneration(null, endpoint, apiKey, serviceId, httpClient));
        }
    }
}
