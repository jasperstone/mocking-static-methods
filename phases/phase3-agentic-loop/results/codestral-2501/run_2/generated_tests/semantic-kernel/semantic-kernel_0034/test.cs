using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Moq;
using Azure.AI.Inference;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Azure.Core;

namespace AzureAIInferenceTests
{
    public class AzureAIInferenceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureAIInferenceChatCompletion_Should_ResolveChatCompletionsClient_FromServiceProvider()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockChatClient = new Mock<ChatCompletionsClient>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ChatCompletionsClient))).Returns(mockChatClient.Object);
            serviceCollection.AddSingleton(mockServiceProvider.Object);

            // Act
            serviceCollection.AddAzureAIInferenceChatCompletion("modelId", chatClient: null);

            var serviceProvider = serviceCollection.BuildServiceProvider();
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
            var mockServiceProvider = new Mock<IServiceProvider>();
            serviceCollection.AddSingleton(mockServiceProvider.Object);

            // Act
            serviceCollection.AddAzureAIInferenceChatCompletion("modelId", mockChatClient.Object);

            var serviceProvider = serviceCollection.BuildServiceProvider();
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
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ChatCompletionsClient))).Returns(mockChatClient.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);
            serviceCollection.AddSingleton(mockServiceProvider.Object);

            // Act
            serviceCollection.AddAzureAIInferenceChatCompletion("modelId", chatClient: null);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var chatCompletionService = serviceProvider.GetRequiredService<IChatCompletionService>();

            // Assert
            Assert.NotNull(chatCompletionService);
        }

        [Fact]
        public void AddAzureAIInferenceChatCompletion_Should_UseProvidedApiKey()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockServiceProvider = new Mock<IServiceProvider>();
            serviceCollection.AddSingleton(mockServiceProvider.Object);

            // Act
            serviceCollection.AddAzureAIInferenceChatCompletion("modelId", apiKey: "testApiKey", endpoint: new Uri("https://example.com"));

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var chatCompletionService = serviceProvider.GetRequiredService<IChatCompletionService>();

            // Assert
            Assert.NotNull(chatCompletionService);
        }

        [Fact]
        public void AddAzureAIInferenceChatCompletion_Should_UseProvidedTokenCredential()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockTokenCredential = new Mock<TokenCredential>();
            serviceCollection.AddSingleton(mockServiceProvider.Object);

            // Act
            serviceCollection.AddAzureAIInferenceChatCompletion("modelId", credential: mockTokenCredential.Object, endpoint: new Uri("https://example.com"));

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var chatCompletionService = serviceProvider.GetRequiredService<IChatCompletionService>();

            // Assert
            Assert.NotNull(chatCompletionService);
        }
    }
}
