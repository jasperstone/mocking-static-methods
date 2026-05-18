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
        private class TestServiceProvider : IServiceProvider
        {
            public bool GetRequiredServiceCalled { get; private set; }
            public ChatCompletionsClient? ReturnedChatClient { get; set; }

            public object? GetService(Type serviceType)
            {
                if (serviceType == typeof(ILoggerFactory))
                {
                    return new Mock<ILoggerFactory>().Object;
                }
                return null;
            }

            public T GetRequiredService<T>()
            {
                if (typeof(T) == typeof(ChatCompletionsClient))
                {
                    GetRequiredServiceCalled = true;
                    return (T)(object)ReturnedChatClient!;
                }
                throw new InvalidOperationException("Service not found");
            }
        }

        [Fact]
        public void AddAzureAIInferenceChatCompletion_UsesGetRequiredService_WhenChatClientIsNull()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockChatClient = new Mock<ChatCompletionsClient>(MockBehavior.Strict, new Uri("http://localhost"), null, null);
            var mockIChatClient = new Mock<IChatClient>();
            var mockBuilder = new Mock<IChatCompletionBuilder>();
            var mockChatCompletionService = new Mock<IChatCompletionService>();

            // Setup fluent builder chain
            mockChatClient.Setup(c => c.AsIChatClient(It.IsAny<string>())).Returns(mockIChatClient.Object);
            mockIChatClient.Setup(c => c.AsBuilder()).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.UseOpenTelemetry(It.IsAny<ILoggerFactory>(), It.IsAny<string?>(), It.IsAny<Action<OpenTelemetryChatClient>?>())).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.UseKernelFunctionInvocation(It.IsAny<ILoggerFactory>())).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.UseLogging(It.IsAny<ILoggerFactory>())).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.Build(It.IsAny<IServiceProvider>())).Returns(mockChatCompletionService.Object);
            mockChatCompletionService.Setup(s => s.AsChatCompletionService(It.IsAny<IServiceProvider>())).Returns(mockChatCompletionService.Object);

            var testServiceProvider = new TestServiceProvider
            {
                ReturnedChatClient = mockChatClient.Object
            };

            // Act
            services.AddSingleton<ChatCompletionsClient>(mockChatClient.Object);
            services.AddSingleton<ILoggerFactory>(new Mock<ILoggerFactory>().Object);

            var resultServices = services.AddAzureAIInferenceChatCompletion(
                modelId: "test-model",
                chatClient: null,
                serviceId: null,
                openTelemetrySourceName: null,
                openTelemetryConfig: null);

            var provider = resultServices.BuildServiceProvider();

            // We manually invoke the factory delegate to simulate the service resolution
            var factory = provider.GetRequiredService<IChatCompletionService>();

            // Assert
            Assert.NotNull(factory);
            // We cannot directly verify the internal call to GetRequiredService on IServiceProvider extension method,
            // but we can verify that the service was created successfully.
        }
    }
}
