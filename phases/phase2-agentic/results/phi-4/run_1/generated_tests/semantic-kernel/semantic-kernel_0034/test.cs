using System;
using System.Net.Http;
using Azure.AI.Inference;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class AzureAIInferenceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureAIInferenceChatCompletion_WhenChatClientIsNull_UsesGetRequiredService()
        {
            // Arrange
            var servicesMock = new Mock<IServiceCollection>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var chatClientMock = new Mock<ChatCompletionsClient>();

            // Setup the service provider to return the mocked ChatCompletionsClient
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<ChatCompletionsClient>())
                .Returns(chatClientMock.Object);

            // Act
            var result = AzureAIInferenceServiceCollectionExtensions.AddAzureAIInferenceChatCompletion(
                servicesMock.Object,
                "modelId",
                chatClient: null);

            // Assert
            servicesMock.Verify(s => s.AddKeyedSingleton<IChatCompletionService>(
                It.IsAny<string>(),
                It.IsAny<Func<IServiceProvider, string, IChatCompletionService>>()), Times.Once);

            serviceProviderMock.Verify(sp => sp.GetRequiredService<ChatCompletionsClient>(), Times.Once);

            Assert.Same(servicesMock.Object, result);
        }
    }
}
