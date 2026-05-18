using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Azure.AI.Inference;
using Moq;

namespace Microsoft.SemanticKernel.Tests
{
    public class AzureAIInferenceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureAIInferenceChatCompletion_ServiceProvider_GetRequiredService_ChatCompletionsClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var chatCompletionsClient = new Mock<ChatCompletionsClient>();
            services.AddSingleton(chatCompletionsClient.Object);

            // Act
            var result = services.AddAzureAIInferenceChatCompletion("modelId", null, "serviceId", null, null);

            // Assert
            Assert.NotNull(result);
            var serviceProviderResult = result.BuildServiceProvider();
            var chatCompletionsClientResult = serviceProviderResult.GetRequiredService<ChatCompletionsClient>();
            Assert.Same(chatCompletionsClient.Object, chatCompletionsClientResult);
        }
    }
}
