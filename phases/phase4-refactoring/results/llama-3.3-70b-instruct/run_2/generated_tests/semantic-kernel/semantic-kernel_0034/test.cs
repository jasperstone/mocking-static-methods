using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Azure.AI.Inference;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureAIInference.Core;
using Microsoft.SemanticKernel;

namespace Connectors.AzureAIInference.Tests
{
    public class AzureAIInferenceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureAIInferenceChatCompletion_ServiceProviderGetRequiredService_ChatCompletionsClientReturned()
        {
            // Arrange
            var services = new ServiceCollection();
            var chatCompletionsClientMock = new Mock<ChatCompletionsClient>();
            services.AddSingleton<ChatCompletionsClient>(chatCompletionsClientMock.Object);
            var serviceProvider = services.BuildServiceProvider();

            // Act
            services.AddAzureAIInferenceChatCompletion("modelId", null, "serviceId");
            var serviceProviderWithChatCompletionsClient = services.BuildServiceProvider();

            // Assert
            var chatCompletionService = serviceProviderWithChatCompletionsClient.GetService<IChatCompletionService>();
            Assert.NotNull(chatCompletionService);
        }

        [Fact]
        public void AddAzureAIInferenceChatCompletion_ServiceProviderGetRequiredService_ThrowsExceptionWhenChatCompletionsClientNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => services.AddAzureAIInferenceChatCompletion("modelId", null, "serviceId"));
        }
    }
}
