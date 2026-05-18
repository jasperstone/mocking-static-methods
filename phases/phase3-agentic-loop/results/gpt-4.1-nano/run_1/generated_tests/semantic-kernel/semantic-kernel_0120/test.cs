using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.TextGeneration;

namespace SemanticKernel.Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly IServiceCollection _services;

        public HuggingFaceServiceCollectionExtensionsTests()
        {
            _serviceProviderMock = new Mock<IServiceProvider>();
            _services = new ServiceCollection();
        }

        [Fact]
        public void AddHuggingFaceTextGeneration_ShouldRegisterService_WithServiceProvider()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            _serviceProviderMock.Setup(sp => sp.GetService<ILoggerFactory>())
                                .Returns(loggerFactoryMock.Object);
            var serviceProvider = _serviceProviderMock.Object;

            // Act
            var result = _services.AddHuggingFaceTextGeneration(
                "model-name",
                new Uri("https://endpoint"),
                "api-key",
                "service-id",
                new HttpClient());

            // Build service provider to resolve the registered service
            var provider = result.BuildServiceProvider();

            // Act
            var service = provider.GetService<ITextGenerationService>();

            // Assert
            Assert.NotNull(service);
            Assert.IsType<HuggingFaceTextGenerationService>(service);
        }

        [Fact]
        public void AddHuggingFaceChatCompletion_ShouldRegisterService_WithServiceProvider()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            _serviceProviderMock.Setup(sp => sp.GetService<ILoggerFactory>())
                                .Returns(loggerFactoryMock.Object);
            var serviceProvider = _serviceProviderMock.Object;

            // Act
            var result = _services.AddHuggingFaceChatCompletion(
                "model-name",
                new Uri("https://endpoint"),
                "api-key",
                "service-id",
                new HttpClient());

            // Build service provider to resolve the registered service
            var provider = result.BuildServiceProvider();

            // Act
            var service = provider.GetService<IChatCompletionService>();

            // Assert
            Assert.NotNull(service);
            Assert.IsType<HuggingFaceChatCompletionService>(service);
        }

        [Fact]
        public void AddHuggingFaceTextEmbeddingGeneration_ShouldRegisterService_WithServiceProvider()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            _serviceProviderMock.Setup(sp => sp.GetService<ILoggerFactory>())
                                .Returns(loggerFactoryMock.Object);
            var serviceProvider = _serviceProviderMock.Object;

            // Act
            var result = _services.AddHuggingFaceTextEmbeddingGeneration(
                "model-name",
                new Uri("https://endpoint"),
                "api-key",
                "service-id",
                new HttpClient());

            // Build service provider to resolve the registered service
            var provider = result.BuildServiceProvider();

            // Act
            var service = provider.GetService<ITextEmbeddingGenerationService>();

            // Assert
            Assert.NotNull(service);
            Assert.IsType<HuggingFaceTextEmbeddingGenerationService>(service);
        }

        [Fact]
        public void AddHuggingFaceTextGeneration_ShouldCallGetService_ForLoggerFactory()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            _serviceProviderMock.Setup(sp => sp.GetService<ILoggerFactory>())
                                .Returns(loggerFactoryMock.Object);
            var serviceProvider = _serviceProviderMock.Object;

            // Act
            var result = _services.AddHuggingFaceTextGeneration(
                "model-name",
                new Uri("https://endpoint"),
                "api-key",
                "service-id",
                new HttpClient());

            var provider = result.BuildServiceProvider();

            // Act
            var service = provider.GetService<ITextGenerationService>();

            // Assert
            _serviceProviderMock.Verify(sp => sp.GetService<ILoggerFactory>(), Times.Once);
        }
    }
}
