using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaTextEmbeddingGeneration_WithProvidedClient_UsesProvidedClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var ollamaClient = new Mock<OllamaApiClient>().Object;
            var serviceProvider = services.BuildServiceProvider();

            // Act
            services.AddOllamaTextEmbeddingGeneration(ollamaClient);

            // Assert
            var resolvedService = services.BuildServiceProvider().GetService<ITextEmbeddingGenerationService>();
            Assert.NotNull(resolvedService);
        }

        [Fact]
        public void AddOllamaTextEmbeddingGeneration_WithClientInServiceProvider_UsesServiceProviderClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var ollamaClient = new Mock<OllamaApiClient>().Object;
            services.AddSingleton<OllamaApiClient>(ollamaClient);
            var serviceProvider = services.BuildServiceProvider();

            // Act
            services.AddOllamaTextEmbeddingGeneration();

            // Assert
            var resolvedService = services.BuildServiceProvider().GetService<ITextEmbeddingGenerationService>();
            Assert.NotNull(resolvedService);
        }

        [Fact]
        public void AddOllamaTextEmbeddingGeneration_WithoutClientOrServiceProvider_ThrowsInvalidOperationException()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => services.AddOllamaTextEmbeddingGeneration());
        }

        [Fact]
        public void AddOllamaTextEmbeddingGeneration_WithIOllamaApiClient_UsesCastedClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var ollamaApiClient = new Mock<IOllamaApiClient>().Object;
            services.AddSingleton<IOllamaApiClient>(ollamaApiClient);
            var serviceProvider = services.BuildServiceProvider();

            // Act
            services.AddOllamaTextEmbeddingGeneration();

            // Assert
            var resolvedService = services.BuildServiceProvider().GetService<ITextEmbeddingGenerationService>();
            Assert.NotNull(resolvedService);
        }

        [Fact]
        public void AddOllamaTextEmbeddingGeneration_WithLoggerFactory_UsesLogging()
        {
            // Arrange
            var services = new ServiceCollection();
            var loggerFactory = new Mock<ILoggerFactory>().Object;
            services.AddSingleton<ILoggerFactory>(loggerFactory);
            var serviceProvider = services.BuildServiceProvider();

            // Act
            services.AddOllamaTextEmbeddingGeneration();

            // Assert
            var resolvedService = services.BuildServiceProvider().GetService<ITextEmbeddingGenerationService>();
            Assert.NotNull(resolvedService);
        }
    }
}
