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
        public void AddOllamaTextEmbeddingGeneration_Should_Call_GetService_For_ILoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            services.AddSingleton(mockLoggerFactory.Object);

            // Act
            var result = services.AddOllamaTextEmbeddingGeneration();

            // Assert
            Assert.NotNull(result);
            var provider = result.BuildServiceProvider();
            var loggerFactory = provider.GetService<ILoggerFactory>();
            Assert.NotNull(loggerFactory);
        }

        [Fact]
        public void AddOllamaChatCompletion_Should_Call_GetService_For_ILoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            services.AddSingleton(mockLoggerFactory.Object);

            // Act
            var result = services.AddOllamaChatCompletion("model", new Uri("http://localhost"));

            // Assert
            Assert.NotNull(result);
            var provider = result.BuildServiceProvider();
            var loggerFactory = provider.GetService<ILoggerFactory>();
            Assert.NotNull(loggerFactory);
        }

        [Fact]
        public void AddOllamaTextGeneration_Should_Throw_If_No_IOllamaApiClient()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                services.AddOllamaTextGeneration());
        }
    }
}
