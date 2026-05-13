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
        public void AddAzureAIInferenceChatCompletion_Should_Throw_If_ServiceProvider_Does_Not_Have_ChatCompletionsClient()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var modelId = "test-model-id";
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                serviceCollection.AddAzureAIInferenceChatCompletion(modelId));
            Assert.Equal("Service of type 'Azure.AI.Inference.ChatCompletionsClient' is not registered.", exception.Message);
        }

        [Fact]
        public void AddAzureAIInferenceChatCompletion_Should_Use_Provided_ChatCompletionsClient()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var modelId = "test-model-id";
            var chatClientMock = new Mock<ChatCompletionsClient>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ChatCompletionsClient))).Returns(chatClientMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(Mock.Of<ILoggerFactory>());

            serviceCollection.AddSingleton(serviceProviderMock.Object);

            // Act
            serviceCollection.AddAzureAIInferenceChatCompletion(modelId, chatClientMock.Object);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var chatCompletionService = serviceProvider.GetRequiredService<IChatCompletionService>();
            Assert.NotNull(chatCompletionService);
        }
    }
}
