using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Azure.AI.Inference;
using Azure.Core;
using Moq;
using System;

namespace Microsoft.SemanticKernel.Tests
{
    public class AzureAIInferenceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureAIInferenceChatCompletion_Should_ResolveChatClientFromServiceProvider()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockChatClient = new Mock<ChatCompletionsClient>();
            serviceCollection.AddSingleton(mockChatClient.Object);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Act
            serviceCollection.AddAzureAIInferenceChatCompletion("modelId");

            var chatCompletionService = serviceProvider.GetRequiredService<IChatCompletionService>();

            // Assert
            Assert.NotNull(chatCompletionService);
        }

        [Fact]
        public void AddAzureAIInferenceChatCompletion_Should_UseProvidedChatClient()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockChatClient = new Mock<ChatCompletionsClient>();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Act
            serviceCollection.AddAzureAIInferenceChatCompletion("modelId", mockChatClient.Object);

            var chatCompletionService = serviceProvider.GetRequiredService<IChatCompletionService>();

            // Assert
            Assert.NotNull(chatCompletionService);
        }

        [Fact]
        public void AddAzureAIInferenceChatCompletion_Should_ThrowIfChatClientNotAvailable()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => serviceCollection.AddAzureAIInferenceChatCompletion("modelId"));
        }

        [Fact]
        public void AddAzureAIInferenceChatCompletion_WithApiKey_Should_ResolveChatClientFromServiceProvider()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockChatClient = new Mock<ChatCompletionsClient>();
            serviceCollection.AddSingleton(mockChatClient.Object);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Act
            serviceCollection.AddAzureAIInferenceChatCompletion("modelId", "apiKey");

            var chatCompletionService = serviceProvider.GetRequiredService<IChatCompletionService>();

            // Assert
            Assert.NotNull(chatCompletionService);
        }

        [Fact]
        public void AddAzureAIInferenceChatCompletion_WithTokenCredential_Should_ResolveChatClientFromServiceProvider()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockChatClient = new Mock<ChatCompletionsClient>();
            serviceCollection.AddSingleton(mockChatClient.Object);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Act
            serviceCollection.AddAzureAIInferenceChatCompletion("modelId", new Mock<TokenCredential>().Object);

            var chatCompletionService = serviceProvider.GetRequiredService<IChatCompletionService>();

            // Assert
            Assert.NotNull(chatCompletionService);
        }
    }
}
