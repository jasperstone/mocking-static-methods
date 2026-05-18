using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaChatCompletion_WithEndpoint_ShouldRegisterService()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "test-model";
            var endpoint = new Uri("https://example.com");

            // Act
            services.AddOllamaChatCompletion(modelId, endpoint);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var chatCompletionService = serviceProvider.GetService<IChatCompletionService>();
            Assert.NotNull(chatCompletionService);
        }

        [Fact]
        public void AddOllamaChatCompletion_WithHttpClient_ShouldRegisterService()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "test-model";
            var httpClient = new HttpClient { BaseAddress = new Uri("https://example.com") };

            // Act
            services.AddOllamaChatCompletion(modelId, httpClient);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var chatCompletionService = serviceProvider.GetService<IChatCompletionService>();
            Assert.NotNull(chatCompletionService);
        }

        [Fact]
        public void AddOllamaChatCompletion_WithLoggerFactory_ShouldUseLogger()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "test-model";
            var endpoint = new Uri("https://example.com");
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            services.AddSingleton(loggerFactoryMock.Object);

            // Act
            services.AddOllamaChatCompletion(modelId, endpoint);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var chatCompletionService = serviceProvider.GetService<IChatCompletionService>();
            Assert.NotNull(chatCompletionService);
            loggerFactoryMock.Verify(f => f.CreateLogger(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
