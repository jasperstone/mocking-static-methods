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
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            services.AddSingleton(loggerFactoryMock.Object);

            // Act
            var result = services.AddOllamaTextEmbeddingGeneration();

            // Assert
            Assert.NotNull(result);
            var sp = result.BuildServiceProvider();
            var loggerFactory = sp.GetService<ILoggerFactory>();
            Assert.NotNull(loggerFactory);
        }

        [Fact]
        public void AddOllamaChatCompletion_Should_Call_GetService_For_ILoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            services.AddSingleton(loggerFactoryMock.Object);

            // Act
            var result = services.AddOllamaChatCompletion("model", new Uri("http://localhost"));

            // Assert
            Assert.NotNull(result);
            var sp = result.BuildServiceProvider();
            var loggerFactory = sp.GetService<ILoggerFactory>();
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

        [Fact]
        public void AddOllamaTextGeneration_Should_Return_Service_When_IOllamaApiClient_Present()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockClient = new Mock<IOllamaApiClient>();
            services.AddSingleton<IOllamaApiClient>(mockClient.Object);

            // Act
            var result = services.AddOllamaTextGeneration();

            // Assert
            Assert.NotNull(result);
            var sp = result.BuildServiceProvider();
            var service = sp.GetService<ITextEmbeddingGenerationService>();
            Assert.NotNull(service);
        }
    }
}
