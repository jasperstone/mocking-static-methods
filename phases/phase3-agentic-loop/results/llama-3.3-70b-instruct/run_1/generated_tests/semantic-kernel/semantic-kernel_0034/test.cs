using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.AzureAIInference;
using Microsoft.SemanticKernel.Connectors.AzureAIInference.Core;
using Moq;
using Xunit;

namespace AzureAIInferenceServiceCollectionExtensionsTests
{
    public class AzureAIInferenceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureAIInferenceChatCompletion_ServiceProviderGetRequiredService_ChatCompletionsClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var chatCompletionsClientMock = new Mock<ChatCompletionsClient>();
            services.AddSingleton(chatCompletionsClientMock.Object);

            // Act
            services.AddAzureAIInferenceChatCompletion("modelId", chatCompletionsClientMock.Object, "serviceId", null, null);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var chatCompletionService = serviceProvider.GetService<IChatCompletionService>();
            Assert.NotNull(chatCompletionService);
        }

        [Fact]
        public void AddAzureAIInferenceChatCompletion_ServiceProviderGetRequiredService_ThrowsException_WhenChatCompletionsClientIsNull()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => services.AddAzureAIInferenceChatCompletion("modelId", null, "serviceId", null, null));
        }
    }
}
