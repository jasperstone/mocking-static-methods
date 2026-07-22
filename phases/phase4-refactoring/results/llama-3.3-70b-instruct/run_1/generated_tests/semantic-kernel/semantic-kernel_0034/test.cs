using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Azure.AI.Inference;

namespace Microsoft.SemanticKernel.Tests
{
    public class AzureAIInferenceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureAIInferenceChatCompletion_WithNullChatClient_ResolvesChatCompletionsClientFromServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTransient<ChatCompletionsClient>(provider => new ChatCompletionsClient(new Azure.AI.Inference.ChatCompletionsClient(new Uri("https://example.com"), new Azure.AzureKeyCredential("key"))));
            var serviceProvider = services.BuildServiceProvider();

            // Act
            services.AddAzureAIInferenceChatCompletion("modelId", null, "serviceId", "openTelemetrySourceName", null);

            // Assert
            var chatCompletionsClient = serviceProvider.GetService<ChatCompletionsClient>();
            Assert.NotNull(chatCompletionsClient);
        }
    }
}
