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
        public void AddHuggingFaceTextGeneration_ShouldRegisterService_WithModel()
        {
            // Arrange
            string model = "test-model";

            // Act
            var result = _services.AddHuggingFaceTextGeneration(model);

            // Assert
            Assert.Same(_services, result);
            var provider = _services.BuildServiceProvider();
            var service = provider.GetService<ITextGenerationService>();
            Assert.NotNull(service);
            Assert.IsType<HuggingFaceTextGenerationService>(service);
        }

        [Fact]
        public void AddHuggingFaceTextGeneration_WithUri_ShouldRegisterService_WithEndpoint()
        {
            // Arrange
            var endpoint = new Uri("https://test-endpoint");

            // Act
            var result = _services.AddHuggingFaceTextGeneration(endpoint);

            // Assert
            Assert.Same(_services, result);
            var provider = _services.BuildServiceProvider();
            var service = provider.GetService<ITextGenerationService>();
            Assert.NotNull(service);
            Assert.IsType<HuggingFaceTextGenerationService>(service);
        }

        [Fact]
        public void AddHuggingFaceChatCompletion_ShouldRegisterService()
        {
            // Arrange
            string model = "chat-model";

            // Act
            var result = _services.AddHuggingFaceChatCompletion(model);

            // Assert
            Assert.Same(_services, result);
            var provider = _services.BuildServiceProvider();
            var service = provider.GetService<IChatCompletionService>();
            Assert.NotNull(service);
            Assert.IsType<HuggingFaceChatCompletionService>(service);
        }

        [Fact]
        public void AddHuggingFaceChatCompletion_WithUri_ShouldRegisterService_WithEndpoint()
        {
            // Arrange
            var endpoint = new Uri("https://test-endpoint");

            // Act
            var result = _services.AddHuggingFaceChatCompletion(endpoint);

            // Assert
            Assert.Same(_services, result);
            var provider = _services.BuildServiceProvider();
            var service = provider.GetService<IChatCompletionService>();
            Assert.NotNull(service);
            Assert.IsType<HuggingFaceChatCompletionService>(service);
        }

        [Fact]
        public void GetService_Returns_Null_When_Service_Not_Registered()
        {
            // Arrange
            var provider = _services.BuildServiceProvider();

            // Act
            var service = provider.GetService<ITextGenerationService>();

            // Assert
            Assert.Null(service);
        }
    }
}
