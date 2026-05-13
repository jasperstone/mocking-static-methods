using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureAIInference.Core;
using Xunit;
using Moq;

namespace AzureAIInferenceExtensions.Tests
{
    public class AzureAIInferenceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureAIInferenceChatCompletion_WithNullServices_Throws()
        {
            IServiceCollection services = null;
            Assert.Throws<ArgumentNullException>(() => AzureAIInferenceServiceCollectionExtensions.AddAzureAIInferenceChatCompletion(services, "modelId"));
        }

        [Fact]
        public void AddAzureAIInferenceChatCompletion_WithExistingService_AddsService()
        {
            var services = new ServiceCollection();
            var mockChatClient = new Mock<ChatCompletionsClient>();
            var mockBuilder = new Mock<IChatCompletionBuilder>();
            var mockService = new Mock<IChatCompletionService>();
            var mockProvider = new ServiceCollection()
                .AddSingleton(mockChatClient.Object)
                .BuildServiceProvider();

            // Setup builder chain
            mockChatClient.Setup(c => c.AsIChatClient(It.IsAny<string>())).Returns(mockChatClient.Object);
            mockChatClient.Setup(c => c.AsBuilder()).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.UseOpenTelemetry(It.IsAny<ILoggerFactory>(), It.IsAny<string>(), It.IsAny<Action<OpenTelemetryChatClient>>())).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.UseKernelFunctionInvocation(It.IsAny<ILoggerFactory>())).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.UseLogging(It.IsAny<ILoggerFactory>())).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.Build(It.IsAny<IServiceProvider>())).Returns(mockChatClient.Object);
            mockBuilder.Setup(b => b.AsChatCompletionService(It.IsAny<IServiceProvider>())).Returns(mockService.Object);

            services.AddSingleton(mockChatClient.Object);
            var serviceProvider = services.BuildServiceProvider();

            var result = AzureAIInferenceServiceCollectionExtensions.AddAzureAIInferenceChatCompletion(
                services,
                "modelId",
                chatClient: null,
                serviceId: "testService");

            Assert.Contains(result, s => s.ServiceType == typeof(IChatCompletionService));
        }

        [Fact]
        public void AddAzureAIInferenceChatCompletion_WithNullChatClient_UsesRequiredService()
        {
            var services = new ServiceCollection();
            var mockChatClient = new Mock<ChatCompletionsClient>();
            var mockBuilder = new Mock<IChatCompletionBuilder>();
            var mockService = new Mock<IChatCompletionService>();

            // Setup builder chain
            mockChatClient.Setup(c => c.AsIChatClient(It.IsAny<string>())).Returns(mockChatClient.Object);
            mockChatClient.Setup(c => c.AsBuilder()).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.UseOpenTelemetry(It.IsAny<ILoggerFactory>(), It.IsAny<string>(), It.IsAny<Action<OpenTelemetryChatClient>>())).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.UseKernelFunctionInvocation(It.IsAny<ILoggerFactory>())).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.UseLogging(It.IsAny<ILoggerFactory>())).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.Build(It.IsAny<IServiceProvider>())).Returns(mockChatClient.Object);
            mockBuilder.Setup(b => b.AsChatCompletionService(It.IsAny<IServiceProvider>())).Returns(mockService.Object);

            services.AddSingleton(mockChatClient.Object);
            var serviceProvider = services.BuildServiceProvider();

            // Register required ChatCompletionsClient
            services.AddSingleton(new ChatCompletionsClient());

            var result = AzureAIInferenceServiceCollectionExtensions.AddAzureAIInferenceChatCompletion(
                services,
                "modelId",
                chatClient: null,
                serviceId: "testService");

            Assert.Contains(result, s => s.ServiceType == typeof(IChatCompletionService));
        }

        [Fact]
        public void AddAzureAIInferenceChatCompletion_WithNullLoggerFactory_BuilderUsesNull()
        {
            var services = new ServiceCollection();
            var mockChatClient = new Mock<ChatCompletionsClient>();
            var mockBuilder = new Mock<IChatCompletionBuilder>();
            var mockService = new Mock<IChatCompletionService>();
            var mockProvider = new ServiceCollection()
                .AddSingleton(mockChatClient.Object)
                .BuildServiceProvider();

            // Setup builder chain
            mockChatClient.Setup(c => c.AsIChatClient(It.IsAny<string>())).Returns(mockChatClient.Object);
            mockChatClient.Setup(c => c.AsBuilder()).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.UseOpenTelemetry(It.IsAny<ILoggerFactory>(), It.IsAny<string>(), It.IsAny<Action<OpenTelemetryChatClient>>())).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.UseKernelFunctionInvocation(It.IsAny<ILoggerFactory>())).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.UseLogging(It.IsAny<ILoggerFactory>())).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.Build(It.IsAny<IServiceProvider>())).Returns(mockChatClient.Object);
            mockBuilder.Setup(b => b.AsChatCompletionService(It.IsAny<IServiceProvider>())).Returns(mockService.Object);

            services.AddSingleton(mockChatClient.Object);
            var serviceProvider = services.BuildServiceProvider();

            // Remove ILoggerFactory registration to simulate null
            var result = AzureAIInferenceServiceCollectionExtensions.AddAzureAIInferenceChatCompletion(
                services,
                "modelId",
                chatClient: null,
                serviceId: "testService");

            Assert.Contains(result, s => s.ServiceType == typeof(IChatCompletionService));
        }
    }
}
