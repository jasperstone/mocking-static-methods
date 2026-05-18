using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Microsoft.Extensions.Logging;
using Azure.AI.Inference;
using Microsoft.SemanticKernel;
using Azure.Core;

namespace Connectors.AzureAIInference.Tests
{
    public class AzureAIInferenceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureAIInferenceChatCompletion_Should_ResolveChatCompletionsClient_FromServiceProvider()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockChatClient = new Mock<ChatCompletionsClient>();
            serviceCollection.AddSingleton(mockChatClient.Object);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Act
            serviceCollection.AddAzureAIInferenceChatCompletion("modelId", chatClient: null);

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
        public void AddAzureAIInferenceChatCompletion_Should_UseLoggerFactory()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockChatClient = new Mock<ChatCompletionsClient>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            serviceCollection.AddSingleton(mockLoggerFactory.Object);
            serviceCollection.AddSingleton(mockChatClient.Object);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Act
            serviceCollection.AddAzureAIInferenceChatCompletion("modelId", chatClient: null);

            var chatCompletionService = serviceProvider.GetRequiredService<IChatCompletionService>();

            // Assert
            Assert.NotNull(chatCompletionService);
        }

        [Fact]
        public void AddAzureAIInferenceChatCompletion_Should_UseProvidedApiKey()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockHttpClient = new Mock<HttpClient>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            serviceCollection.AddSingleton(mockHttpClient.Object);
            serviceCollection.AddSingleton(mockLoggerFactory.Object);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Act
            serviceCollection.AddAzureAIInferenceChatCompletion("modelId", apiKey: "testApiKey");

            var chatCompletionService = serviceProvider.GetRequiredService<IChatCompletionService>();

            // Assert
            Assert.NotNull(chatCompletionService);
        }

        [Fact]
        public void AddAzureAIInferenceChatCompletion_Should_UseProvidedTokenCredential()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockHttpClient = new Mock<HttpClient>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockTokenCredential = new Mock<TokenCredential>();
            serviceCollection.AddSingleton(mockHttpClient.Object);
            serviceCollection.AddSingleton(mockLoggerFactory.Object);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Act
            serviceCollection.AddAzureAIInferenceChatCompletion("modelId", credential: mockTokenCredential.Object);

            var chatCompletionService = serviceProvider.GetRequiredService<IChatCompletionService>();

            // Assert
            Assert.NotNull(chatCompletionService);
        }
    }
}
