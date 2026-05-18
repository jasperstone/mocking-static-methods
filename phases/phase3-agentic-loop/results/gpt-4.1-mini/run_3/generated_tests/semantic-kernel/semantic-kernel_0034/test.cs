using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureAIInference.Extensions;
using Moq;
using Xunit;
using Azure.AI.Inference;

namespace Microsoft.SemanticKernel.Connectors.AzureAIInference.Extensions.Tests
{
    public class AzureAIInferenceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureAIInferenceChatCompletion_UsesGetRequiredService_WhenChatClientIsNull()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockChatClient = new Mock<ChatCompletionsClient>(MockBehavior.Strict, new Uri("http://localhost"), null, null);
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockBuilder = new Mock<IChatCompletionBuilder>();
            var mockIChatClient = new Mock<IChatClient>();
            var mockChatCompletionService = new Mock<IChatCompletionService>();

            // Setup the service provider to return the mock ChatCompletionsClient when GetRequiredService is called
            mockServiceProvider.Setup(sp => sp.GetRequiredService<ChatCompletionsClient>())
                .Returns(mockChatClient.Object);

            // Setup the service provider to return a logger factory
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(mockLoggerFactory.Object);

            // Setup the fluent builder chain
            mockChatClient.Setup(c => c.AsIChatClient(It.IsAny<string>())).Returns(mockIChatClient.Object);
            mockIChatClient.Setup(c => c.AsBuilder()).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.UseOpenTelemetry(mockLoggerFactory.Object, It.IsAny<string?>(), It.IsAny<Action<OpenTelemetryChatClient>?>())).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.UseKernelFunctionInvocation(mockLoggerFactory.Object)).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.UseLogging(mockLoggerFactory.Object)).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.Build(mockServiceProvider.Object)).Returns(mockChatCompletionService.Object);
            mockChatCompletionService.Setup(s => s.AsChatCompletionService(mockServiceProvider.Object)).Returns(mockChatCompletionService.Object);

            // Add the mock service provider to the service collection
            services.AddSingleton(sp => mockServiceProvider.Object);

            // Act
            var result = services.AddAzureAIInferenceChatCompletion(
                modelId: "test-model",
                chatClient: null,
                serviceId: null,
                openTelemetrySourceName: null,
                openTelemetryConfig: null);

            // Build the service provider and resolve the IChatCompletionService to trigger the factory delegate
            var serviceProvider = services.BuildServiceProvider();
            var chatCompletionService = serviceProvider.GetRequiredService<IChatCompletionService>();

            // Assert
            mockServiceProvider.Verify(sp => sp.GetRequiredService<ChatCompletionsClient>(), Times.Once);
            Assert.NotNull(chatCompletionService);
        }
    }
}
