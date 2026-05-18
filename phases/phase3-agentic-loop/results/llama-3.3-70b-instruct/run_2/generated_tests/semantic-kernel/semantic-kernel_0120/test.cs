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
        public async Task AddHuggingFaceTextEmbeddingGeneration_ServiceProvider_GetService_ReturnsILoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            // Act
            var huggingFaceTextEmbeddingGenerationService = new HuggingFaceTextEmbeddingGenerationService(
                new Uri("https://api-inference.huggingface.co/models/model"),
                "apiKey",
                new HttpClient(),
                loggerFactory);

            // Assert
            Assert.NotNull(huggingFaceTextEmbeddingGenerationService);
        }

        [Fact]
        public async Task AddHuggingFaceTextEmbeddingGeneration_ServiceProvider_GetService_ThrowsException_WhenLoggerFactoryIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = (ILoggerFactory)null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => new HuggingFaceTextEmbeddingGenerationService(
                new Uri("https://api-inference.huggingface.co/models/model"),
                "apiKey",
                new HttpClient(),
                loggerFactory));
        }

        [Fact]
        public async Task AddHuggingFaceImageToText_ServiceProvider_GetService_ReturnsILoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            // Act
            var huggingFaceImageToTextService = new HuggingFaceImageToTextService(
                "model",
                new Uri("https://api-inference.huggingface.co/models/model"),
                "apiKey",
                new HttpClient(),
                loggerFactory);

            // Assert
            Assert.NotNull(huggingFaceImageToTextService);
        }

        [Fact]
        public async Task AddHuggingFaceImageToText_ServiceProvider_GetService_ThrowsException_WhenLoggerFactoryIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = (ILoggerFactory)null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => new HuggingFaceImageToTextService(
                "model",
                new Uri("https://api-inference.huggingface.co/models/model"),
                "apiKey",
                new HttpClient(),
                loggerFactory));
        }
    }
}
