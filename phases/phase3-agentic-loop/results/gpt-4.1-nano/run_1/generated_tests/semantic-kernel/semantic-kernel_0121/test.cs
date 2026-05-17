using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Http;
using Microsoft.SemanticKernel.ImageToText;
using Microsoft.SemanticKernel.TextGeneration;
using Microsoft.SemanticKernel.Connectors.HuggingFace;

namespace SemanticKernel.Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        private readonly ServiceCollection _services;
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;

        public HuggingFaceServiceCollectionExtensionsTests()
        {
            _services = new ServiceCollection();
            _loggerFactoryMock = new Mock<ILoggerFactory>();
        }

        [Fact]
        public void AddHuggingFaceTextGeneration_ShouldRegisterService_WithModelAndEndpoint()
        {
            // Arrange
            var model = "test-model";
            var endpoint = new Uri("https://test-endpoint");
            var apiKey = "test-api-key";

            // Act
            _services.AddHuggingFaceTextGeneration(model, endpoint, apiKey);

            // Build service provider
            var provider = _services.BuildServiceProvider();

            // Assert
            var service = provider.GetService<ITextGenerationService>();
            Assert.NotNull(service);
            Assert.IsType<HuggingFaceTextGenerationService>(service);
        }

        [Fact]
        public void AddHuggingFaceTextGeneration_ShouldCallGetService_AndUseLoggerFactory()
        {
            // Arrange
            var model = "model";
            var endpoint = new Uri("https://endpoint");
            var services = new ServiceCollection();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            services.AddSingleton(loggerFactoryMock.Object);

            // Act
            services.AddHuggingFaceTextGeneration(model, endpoint, null);
            var provider = services.BuildServiceProvider();

            // Retrieve the service
            var service = provider.GetService<ITextGenerationService>();
            Assert.NotNull(service);
            Assert.IsType<HuggingFaceTextGenerationService>(service);
        }

        [Fact]
        public void AddHuggingFaceChatCompletion_ShouldRegisterService_WithModelAndEndpoint()
        {
            // Arrange
            var model = "chat-model";
            var endpoint = new Uri("https://chat-endpoint");
            var apiKey = "chat-api-key";

            // Act
            _services.AddHuggingFaceChatCompletion(model, endpoint, apiKey);

            // Build provider
            var provider = _services.BuildServiceProvider();

            // Assert
            var service = provider.GetService<IChatCompletionService>();
            Assert.NotNull(service);
            Assert.IsType<HuggingFaceChatCompletionService>(service);
        }

        [Fact]
        public void AddHuggingFaceChatCompletion_ShouldCallGetService_AndUseLoggerFactory()
        {
            // Arrange
            var endpoint = new Uri("https://chat-endpoint");
            var services = new ServiceCollection();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            services.AddSingleton(loggerFactoryMock.Object);

            // Act
            services.AddHuggingFaceChatCompletion(endpoint);
            var provider = services.BuildServiceProvider();

            // Assert
            var service = provider.GetService<IChatCompletionService>();
            Assert.NotNull(service);
            Assert.IsType<HuggingFaceChatCompletionService>(service);
        }

        [Fact]
        public void AddHuggingFaceTextEmbeddingGeneration_ShouldRegisterService_WithModelAndEndpoint()
        {
            // Arrange
            var model = "embedding-model";
            var endpoint = new Uri("https://embedding-endpoint");
            var apiKey = "embedding-api-key";

            // Act
            _services.AddHuggingFaceTextEmbeddingGeneration(model, endpoint, apiKey);

            // Build provider
            var provider = _services.BuildServiceProvider();

            // Assert
            var service = provider.GetService<ITextEmbeddingGenerationService>();
            Assert.NotNull(service);
            Assert.IsType<HuggingFaceTextEmbeddingGenerationService>(service);
        }

        [Fact]
        public void AddHuggingFaceTextEmbeddingGeneration_ShouldCallGetService_AndUseLoggerFactory()
        {
            // Arrange
            var endpoint = new Uri("https://embedding-endpoint");
            var services = new ServiceCollection();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            services.AddSingleton(loggerFactoryMock.Object);

            // Act
            services.AddHuggingFaceTextEmbeddingGeneration("model", endpoint);
            var provider = services.BuildServiceProvider();

            // Assert
            var service = provider.GetService<ITextEmbeddingGenerationService>();
            Assert.NotNull(service);
            Assert.IsType<HuggingFaceTextEmbeddingGenerationService>(service);
        }
    }
}
