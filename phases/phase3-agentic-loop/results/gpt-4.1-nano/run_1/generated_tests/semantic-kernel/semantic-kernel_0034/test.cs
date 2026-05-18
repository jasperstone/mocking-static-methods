using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class AzureAIInferenceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureAIInferenceChatCompletion_Should_Call_GetRequiredService_ForChatClient_When_NotProvided()
        {
            // Arrange
            var services = new ServiceCollection();
            var chatClientMock = new Mock<ChatCompletionsClient>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var chatClient = chatClientMock.Object;

            // Setup service provider to return chatClient when GetRequiredService is called
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ChatCompletionsClient>())
                .Returns(chatClient);

            // Setup service provider to return loggerFactory
            serviceProviderMock.Setup(sp => sp.GetService<ILoggerFactory>())
                .Returns(loggerFactoryMock.Object);

            // Register the service provider in the service collection
            services.AddSingleton(serviceProviderMock.Object);

            // Act
            services.AddAzureAIInferenceChatCompletion(
                modelId: "model-id",
                chatClient: null,
                serviceId: "test-service");

            // Build the service provider
            var provider = services.BuildServiceProvider();

            // Resolve the service to trigger the lambda
            var service = provider.GetService<IChatCompletionService>();

            // Assert
            // Verify that GetRequiredService<ChatCompletionsClient> was called exactly once
            serviceProviderMock.Verify(sp => sp.GetRequiredService<ChatCompletionsClient>(), Times.Once);
        }
    }
}
