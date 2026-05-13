using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaTextEmbeddingGeneration_WhenOllamaClientProvided_UsesProvidedClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var ollamaClientMock = new Mock<OllamaApiClient>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService<ILoggerFactory>()).Returns(null);
            serviceProviderMock.Setup(sp => sp.GetService<OllamaApiClient>()).Returns(ollamaClientMock.Object);

            // Act
            services.AddOllamaTextEmbeddingGeneration(ollamaClientMock.Object);

            // Assert
            var provider = services.BuildServiceProvider();
            var service = provider.GetRequiredService<ITextEmbeddingGenerationService>();
            Assert.Same(ollamaClientMock.Object, service);
        }

        [Fact]
        public void AddOllamaTextEmbeddingGeneration_WhenOllamaClientResolvedFromServiceProvider_UsesResolvedClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var ollamaClientMock = new Mock<OllamaApiClient>();
            services.AddSingleton<OllamaApiClient>(ollamaClientMock.Object);
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService<ILoggerFactory>()).Returns(null);
            serviceProviderMock.Setup(sp => sp.GetService<OllamaApiClient>()).Returns(ollamaClientMock.Object);

            // Act
            services.AddOllamaTextEmbeddingGeneration();

            // Assert
            var provider = services.BuildServiceProvider();
            var service = provider.GetRequiredService<ITextEmbeddingGenerationService>();
            Assert.Same(ollamaClientMock.Object, service);
        }

        [Fact]
        public void AddOllamaTextEmbeddingGeneration_WhenOllamaClientNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService<ILoggerFactory>()).Returns(null);
            serviceProviderMock.Setup(sp => sp.GetService<OllamaApiClient>()).Returns((OllamaApiClient)null);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                services.AddOllamaTextEmbeddingGeneration(serviceProvider: serviceProviderMock.Object));

            Assert.Equal("No IOllamaApiClient implementations found in the service collection.", exception.Message);
        }
    }
}
