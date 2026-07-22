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
        public void AddAzureAIInferenceChatCompletion_WithChatClient_ShouldCallGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockChatClient = new Mock<ChatCompletionsClient>();
            var mockBuilder = new Mock<IChatCompletionBuilder>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();

            // Setup the service provider to return the mock ChatClient
            mockServiceProvider.Setup(sp => sp.GetRequiredService<ChatCompletionsClient>())
                .Returns(mockChatClient.Object);
            mockServiceProvider.Setup(sp => sp.GetService<ILoggerFactory>())
                .Returns(mockLoggerFactory.Object);

            // Setup the builder chain
            mockChatClient.Setup(c => c.AsIChatClient(It.IsAny<string>())).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.AsBuilder()).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.UseOpenTelemetry(It.IsAny<ILoggerFactory>(), It.IsAny<string>(), It.IsAny<Action<OpenTelemetryChatClient>>()))
                .Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.UseKernelFunctionInvocation(It.IsAny<ILoggerFactory>())).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.UseLogging(It.IsAny<ILoggerFactory>())).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.Build(It.IsAny<IServiceProvider>())).Returns(Mock.Of<IChatCompletionService>());

            // Act
            services.AddKeyedSingleton<IChatCompletionService>("testService", (sp, _) =>
            {
                var chatClient = sp.GetRequiredService<ChatCompletionsClient>();
                var builder = chatClient.AsIChatClient("modelId")
                    .AsBuilder()
                    .UseOpenTelemetry(sp.GetService<ILoggerFactory>(), null, null)
                    .UseKernelFunctionInvocation(sp.GetService<ILoggerFactory>());

                var loggerFactory = sp.GetService<ILoggerFactory>();
                if (loggerFactory != null)
                {
                    builder.UseLogging(loggerFactory);
                }

                return builder.Build(sp).AsChatCompletionService(sp);
            });

            // Verify that GetRequiredService<ChatCompletionsClient>() was called
            mockServiceProvider.Verify(sp => sp.GetRequiredService<ChatCompletionsClient>(), Times.Once);
        }
    }
}
