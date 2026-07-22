using System;
using System.Net.Http;
using Azure;
using Azure.AI.Inference;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.AzureAIInference.UnitTests
{
    public class AzureAIInferenceServiceCollectionExtensionsTests
    {
        private class FakeChatCompletionsClient : ChatCompletionsClient
        {
            public FakeChatCompletionsClient() : base(new Uri("http://localhost"), new AzureKeyCredential("key"), new ChatClientOptions())
            {
            }
        }

        [Fact]
        public void AddAzureAIInferenceChatCompletion_WithProvidedChatClient_DoesNotCallGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            var chatClient = new FakeChatCompletionsClient();

            // Act
            var result = AzureAIInferenceServiceCollectionExtensions.AddAzureAIInferenceChatCompletion(
                services,
                "modelId",
                chatClient,
                serviceId: "testService");

            // Assert
            Assert.Same(services, result);
        }

        [Fact]
        public void AddAzureAIInferenceChatCompletion_WithNullChatClient_CallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            var chatClientMock = new Mock<ChatCompletionsClient>(new Uri("http://localhost"), new AzureKeyCredential("key"), new ChatClientOptions());
            services.AddSingleton(chatClientMock.Object);

            AzureAIInferenceServiceCollectionExtensions.AddAzureAIInferenceChatCompletion(
                services,
                "modelId",
                chatClient: null,
                serviceId: "testService");

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var chatCompletionService = serviceProvider.GetRequiredService<IChatCompletionService>();

            // Assert
            Assert.NotNull(chatCompletionService);
        }
    }
}
