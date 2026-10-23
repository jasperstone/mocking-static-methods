using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using System;
using Azure.AI.Inference;

namespace Microsoft.SemanticKernel.Tests
{
    public class AzureAIInferenceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureAIInferenceChatCompletion_Should_ResolveChatCompletionService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockChatClient = new Mock<ChatCompletionsClient>();
            serviceCollection.AddSingleton(mockChatClient.Object);

            // Act
            serviceCollection.AddAzureAIInferenceChatCompletion("modelId", mockChatClient.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var chatCompletionService = serviceProvider.GetRequiredService<IChatCompletionService>();

            // Assert
            Assert.NotNull(chatCompletionService);
        }

        [Fact]
        public void AddAzureAIInferenceChatCompletion_Should_Throw_If_ChatClient_Not_Registered()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                serviceCollection.AddAzureAIInferenceChatCompletion("modelId");
                var serviceProvider = serviceCollection.BuildServiceProvider();
                serviceProvider.GetRequiredService<IChatCompletionService>();
            });
        }
    }
}
