using System;
using Azure.AI.Inference;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class AzureAIInferenceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureAIInferenceChatCompletion_Should_Throw_If_ChatClient_Not_Registered()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var modelId = "test-model-id";
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                serviceCollection.AddAzureAIInferenceChatCompletion(modelId));
            Assert.Contains("ChatCompletionsClient", exception.Message);
        }

        [Fact]
        public void AddAzureAIInferenceChatCompletion_Should_Use_Provided_ChatClient()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var modelId = "test-model-id";
            var chatClientMock = new Mock<ChatCompletionsClient>();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Act
            serviceCollection.AddAzureAIInferenceChatCompletion(modelId, chatClientMock.Object);
            var service = serviceProvider.GetRequiredService<IChatCompletionService>();

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void AddAzureAIInferenceChatCompletion_Should_Use_Registered_ChatClient()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var modelId = "test-model-id";
            var chatClientMock = new Mock<ChatCompletionsClient>();
            serviceCollection.AddSingleton(chatClientMock.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Act
            serviceCollection.AddAzureAIInferenceChatCompletion(modelId);
            var service = serviceProvider.GetRequiredService<IChatCompletionService>();

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void AddAzureAIInferenceChatCompletion_Should_Use_LoggerFactory()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var modelId = "test-model-id";
            var chatClientMock = new Mock<ChatCompletionsClient>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceCollection.AddSingleton(chatClientMock.Object);
            serviceCollection.AddSingleton(loggerFactoryMock.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Act
            serviceCollection.AddAzureAIInferenceChatCompletion(modelId);
            var service = serviceProvider.GetRequiredService<IChatCompletionService>();

            // Assert
            Assert.NotNull(service);
        }
    }
}
