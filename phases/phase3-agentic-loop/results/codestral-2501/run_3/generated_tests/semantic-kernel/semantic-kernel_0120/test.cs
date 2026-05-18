using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Http;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHuggingFaceTextEmbeddingGeneration_ShouldRegisterService()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var httpClientMock = new Mock<HttpClient>();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(HttpClient)))
                .Returns(httpClientMock.Object);

            var serviceProvider = serviceProviderMock.Object;

            // Act
            services.AddHuggingFaceTextEmbeddingGeneration(
                new Uri("https://api.huggingface.co"),
                "apiKey",
                "serviceId",
                httpClientMock.Object);

            var service = services.BuildServiceProvider().GetService<ITextEmbeddingGenerationService>();

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void AddHuggingFaceTextEmbeddingGeneration_ShouldCallGetService()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var httpClientMock = new Mock<HttpClient>();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(HttpClient)))
                .Returns(httpClientMock.Object);

            var serviceProvider = serviceProviderMock.Object;

            // Act
            services.AddHuggingFaceTextEmbeddingGeneration(
                new Uri("https://api.huggingface.co"),
                "apiKey",
                "serviceId",
                httpClientMock.Object);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
        }

        [Fact]
        public void AddHuggingFaceTextEmbeddingGeneration_ShouldCallGetHttpClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var httpClientMock = new Mock<HttpClient>();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(HttpClient)))
                .Returns(httpClientMock.Object);

            var serviceProvider = serviceProviderMock.Object;

            // Act
            services.AddHuggingFaceTextEmbeddingGeneration(
                new Uri("https://api.huggingface.co"),
                "apiKey",
                "serviceId",
                httpClientMock.Object);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(HttpClient)), Times.Once);
        }
    }
}
