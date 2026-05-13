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
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;

        public HuggingFaceServiceCollectionExtensionsTests()
        {
            _services = new ServiceCollection();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _serviceProviderMock.Setup(sp => sp.GetService<ILoggerFactory>()).Returns(_loggerFactoryMock.Object);
        }

        [Fact]
        public void AddHuggingFaceTextGeneration_ShouldRegisterService()
        {
            // Arrange
            var model = "test-model";

            // Act
            var result = _services.AddHuggingFaceTextGeneration(model);

            // Assert
            Assert.Same(_services, result);
            var serviceProvider = _services.BuildServiceProvider();
            var service = serviceProvider.GetService<ITextGenerationService>();
            Assert.NotNull(service);
            Assert.IsType<HuggingFaceTextGenerationService>(service);
        }

        [Fact]
        public void AddHuggingFaceTextGeneration_WithUri_ShouldRegisterService()
        {
            // Arrange
            var endpoint = new Uri("https://test-endpoint");
            // Act
            var result = _services.AddHuggingFaceTextGeneration(endpoint);
            // Assert
            Assert.Same(_services, result);
            var serviceProvider = _services.BuildServiceProvider();
            var service = serviceProvider.GetService<ITextGenerationService>();
            Assert.NotNull(service);
            Assert.IsType<HuggingFaceTextGenerationService>(service);
        }

        [Fact]
        public void AddHuggingFaceChatCompletion_ShouldRegisterService()
        {
            // Arrange
            var model = "chat-model";

            // Act
            var result = _services.AddHuggingFaceChatCompletion(model);

            // Assert
            Assert.Same(_services, result);
            var serviceProvider = _services.BuildServiceProvider();
            var service = serviceProvider.GetService<IChatCompletionService>();
            Assert.NotNull(service);
            Assert.IsType<HuggingFaceChatCompletionService>(service);
        }

        [Fact]
        public void AddHuggingFaceChatCompletion_WithUri_ShouldRegisterService()
        {
            // Arrange
            var endpoint = new Uri("https://test-endpoint");

            // Act
            var result = _services.AddHuggingFaceChatCompletion(endpoint);

            // Assert
            Assert.Same(_services, result);
            var serviceProvider = _services.BuildServiceProvider();
            var service = serviceProvider.GetService<IChatCompletionService>();
            Assert.NotNull(service);
            Assert.IsType<HuggingFaceChatCompletionService>(service);
        }

        [Fact]
        public void AddHuggingFaceTextEmbeddingGeneration_ShouldRegisterService()
        {
            // Arrange
            var model = "embedding-model";

            // Act
            var result = _services.AddHuggingFaceTextEmbeddingGeneration(model);

            // Assert
            Assert.Same(_services, result);
            var serviceProvider = _services.BuildServiceProvider();
            var service = serviceProvider.GetService<ITextEmbeddingGenerationService>();
            Assert.NotNull(service);
            Assert.IsType<HuggingFaceTextEmbeddingGenerationService>(service);
        }

        [Fact]
        public void AddHuggingFaceTextEmbeddingGeneration_WithUri_ShouldRegisterService()
        {
            // Arrange
            var endpoint = new Uri("https://test-endpoint");

            // Act
            var result = _services.AddHuggingFaceTextEmbeddingGeneration(endpoint);

            // Assert
            Assert.Same(_services, result);
            var serviceProvider = _services.BuildServiceProvider();
            var service = serviceProvider.GetService<ITextEmbeddingGenerationService>();
            Assert.NotNull(service);
            Assert.IsType<HuggingFaceTextEmbeddingGenerationService>(service);
        }

        [Fact]
        public void AddHuggingFaceTextEmbeddingGeneration_ShouldUseGetService()
        {
            // Arrange
            var endpoint = new Uri("https://test-endpoint");
            _services.AddTransient<IServiceProvider>(_ => _serviceProviderMock.Object);
            var serviceProvider = _services.BuildServiceProvider();

            // Act
            var service = serviceProvider.GetService<IServiceProvider>();
            var result = _services.AddHuggingFaceTextEmbeddingGeneration("model", endpoint);

            // Assert
            Assert.Same(_services, result);
            var registeredService = serviceProvider.GetService<ITextEmbeddingGenerationService>();
            Assert.NotNull(registeredService);
        }
    }
}
