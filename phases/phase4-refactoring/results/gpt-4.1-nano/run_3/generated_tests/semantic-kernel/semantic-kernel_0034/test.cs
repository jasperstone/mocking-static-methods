using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;

namespace AzureAIInferenceTests
{
    public class AzureAIInferenceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureAIInferenceChatCompletion_Should_Call_GetRequiredService_ForChatCompletionsClient()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockChatClient = new Mock<ChatCompletionsClient>();
            var mockBuilder = new Mock<ChatCompletionsClient.IBuilder>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();

            // Setup the service provider to return the mock ChatCompletionsClient
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
            mockBuilder.Setup(b => b.Build(It.IsAny<IServiceProvider>())).Returns(Mock.Of<ChatCompletionService>());
            mockBuilder.Setup(b => b.AsChatCompletionService(It.IsAny<IServiceProvider>())).Returns(Mock.Of<IChatCompletionService>());

            // Act
            services.AddAzureAIInferenceChatCompletion("modelId", chatClient: mockChatClient.Object);
            var serviceProvider = services.BuildServiceProvider();

            // Manually invoke the registration callback to simulate the service registration
            var registration = services[0] as ServiceDescriptor;
            var factory = registration?.ImplementationFactory;
            factory?.Invoke(serviceProvider);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetRequiredService<ChatCompletionsClient>(), Times.Once);
        }
    }
}
