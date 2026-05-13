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
        public void AddAzureAIInferenceChatCompletion_Should_Throw_If_ChatClient_Not_Provided_And_Not_In_ServiceProvider()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var modelId = "test-model-id";

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
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(Mock.Of<ILoggerFactory>());

            // Act
            serviceCollection.AddAzureAIInferenceChatCompletion(modelId, chatClientMock.Object);
            var serviceProvider = serviceProviderMock.Object;
            var chatCompletionService = serviceProvider.GetRequiredService<IChatCompletionService>();

            // Assert
            Assert.NotNull(chatCompletionService);
        }

        [Fact]
        public void AddAzureAIInferenceChatCompletion_Should_Get_ChatClient_From_ServiceProvider()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var modelId = "test-model-id";
            var chatClientMock = new Mock<ChatCompletionsClient>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(Mock.Of<ILoggerFactory>());
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(ChatCompletionsClient))).Returns(chatClientMock.Object);

            // Act
            serviceCollection.AddAzureAIInferenceChatCompletion(modelId);
            var serviceProvider = serviceProviderMock.Object;
            var chatCompletionService = serviceProvider.GetRequiredService<IChatCompletionService>();

            // Assert
            Assert.NotNull(chatCompletionService);
        }
    }
}
