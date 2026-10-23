using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Azure.AI.Inference;
using Microsoft.SemanticKernel;

namespace Connectors.AzureAIInference.Tests
{
    public class AzureAIInferenceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureAIInferenceChatCompletion_GetRequiredServiceCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var chatCompletionsClientMock = new Mock<ChatCompletionsClient>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ChatCompletionsClient>()).Returns(chatCompletionsClientMock.Object);

            // Act
            services.AddAzureAIInferenceChatCompletion("modelId", chatCompletionsClientMock.Object, "serviceId");

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<ChatCompletionsClient>(), Times.Once);
        }
    }
}
