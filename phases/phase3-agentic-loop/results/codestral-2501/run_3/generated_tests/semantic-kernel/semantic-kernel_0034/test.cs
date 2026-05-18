using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Moq;
using Azure.AI.Inference;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Connectors.AzureAIInference.Tests
{
    public class AzureAIInferenceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureAIInferenceChatCompletion_Should_ResolveChatCompletionService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockChatClient = new Mock<ChatCompletionsClient>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ChatCompletionsClient))).Returns(mockChatClient.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(Mock.Of<ILoggerFactory>());

            serviceCollection.AddSingleton(mockServiceProvider.Object);

            // Act
            serviceCollection.AddAzureAIInferenceChatCompletion("modelId", mockChatClient.Object);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var chatCompletionService = serviceProvider.GetRequiredService<IChatCompletionService>();

            // Assert
            Assert.NotNull(chatCompletionService);
        }

        [Fact]
        public void AddAzureAIInferenceChatCompletion_Should_Throw_When_ChatClient_Not_Registered()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ChatCompletionsClient))).Returns((ChatCompletionsClient)null);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(Mock.Of<ILoggerFactory>());

            serviceCollection.AddSingleton(mockServiceProvider.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => serviceCollection.AddAzureAIInferenceChatCompletion("modelId"));
        }

        [Fact]
        public void AddAzureAIInferenceChatCompletion_Should_UseChatClientFromServiceProvider()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockChatClient = new Mock<ChatCompletionsClient>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetRequiredService(typeof(ChatCompletionsClient))).Returns(mockChatClient.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(Mock.Of<ILoggerFactory>());

            serviceCollection.AddSingleton(mockServiceProvider.Object);

            // Act
            serviceCollection.AddAzureAIInferenceChatCompletion("modelId");

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var chatCompletionService = serviceProvider.GetRequiredService<IChatCompletionService>();

            // Assert
            Assert.NotNull(chatCompletionService);
            mockChatClient.Verify(c => c.AsIChatClient("modelId"), Times.Once);
        }
    }
}
