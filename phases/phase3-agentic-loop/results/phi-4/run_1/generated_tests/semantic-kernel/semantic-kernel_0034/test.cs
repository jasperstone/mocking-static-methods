using System;
using System.Net.Http;
using Azure.AI.Inference;
using Azure.Identity;
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
            var services = new ServiceCollection();
            services.AddSingleton<ChatCompletionsClient>(new ChatCompletionsClient(new Uri("https://example.com"), new AzureKeyCredential("fake-key")));
            var serviceProvider = services.BuildServiceProvider();

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider
                .Setup(sp => sp.GetRequiredService<ChatCompletionsClient>())
                .Returns(serviceProvider.GetRequiredService<ChatCompletionsClient>());

            // Act
            var result = AzureAIInferenceServiceCollectionExtensions.AddAzureAIInferenceChatCompletion(
                services,
                "model-id",
                chatClient: null);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetRequiredService<ChatCompletionsClient>(), Times.Once);
            Assert.Same(services, result);
        }
    }
}
