using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.AI;
using Azure.AI.Inference;

namespace Microsoft.SemanticKernel.Tests
{
    public class AzureAIInferenceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureAIInferenceChatCompletion_UsesGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockChatClient = new Mock<ChatCompletionsClient>();
            var mockBuilder = new Mock<IChatClientBuilder>();
            var mockService = new Mock<IChatCompletionService>();

            // Setup the builder chain
            mockChatClient.Setup(c => c.AsIChatClient(It.IsAny<string>())).Returns(mockChatClient.Object);
            mockChatClient.Setup(c => c.AsBuilder()).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.UseOpenTelemetry(It.IsAny<ILoggerFactory>(), It.IsAny<string>(), It.IsAny<Action<OpenTelemetryChatClient>>())).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.UseKernelFunctionInvocation(It.IsAny<ILoggerFactory>())).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.UseLogging(It.IsAny<ILoggerFactory>())).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.Build(It.IsAny<IServiceProvider>())).Returns(mockChatClient.Object);
            mockChatClient.Setup(c => c.AsChatCompletionService(It.IsAny<IServiceProvider>())).Returns(mockService.Object);

            // Register the ChatCompletionsClient as required
            var servicesMock = new ServiceCollection();
            servicesMock.AddTransient(_ => mockChatClient.Object);

            // Act
            services.AddTransient(_ => mockChatClient.Object);
            var provider = services.BuildServiceProvider();

            // Inject the mock into the service collection
            services.AddSingleton(mockChatClient.Object);

            // Call the extension method
            var result = services.AddAzureAIInferenceChatCompletion(
                "modelId",
                chatClient: null,
                serviceId: "testService");

            // Assert
            Assert.Contains(result, s => s == services);
        }
    }
}
