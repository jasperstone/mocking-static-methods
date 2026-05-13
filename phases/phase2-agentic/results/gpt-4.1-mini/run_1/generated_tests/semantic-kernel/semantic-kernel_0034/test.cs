using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureAIInference.Core;
using Moq;
using Xunit;
using Azure.AI.Inference;

namespace Microsoft.SemanticKernel.Tests.Connectors.AzureAIInference.Extensions
{
    public class AzureAIInferenceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureAIInferenceChatCompletion_UsesProvidedChatClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "test-model";
            var mockChatClient = new Mock<ChatCompletionsClient>(MockBehavior.Strict, new Uri("http://localhost"), new Azure.AzureKeyCredential("key"), null);
            var mockBuilder = new Mock<IChatClientBuilder>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();

            // Setup extension methods on ChatCompletionsClient
            mockChatClient.Setup(c => c.AsIChatClient(modelId)).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.AsBuilder()).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.UseOpenTelemetry(It.IsAny<ILoggerFactory>(), It.IsAny<string?>(), It.IsAny<Action<OpenTelemetryChatClient>?>())).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.UseKernelFunctionInvocation(It.IsAny<ILoggerFactory>())).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.UseLogging(It.IsAny<ILoggerFactory>())).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.Build(It.IsAny<IServiceProvider>())).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.AsChatCompletionService(It.IsAny<IServiceProvider>())).Returns(Mock.Of<IChatCompletionService>());

            // Setup service provider to return null for ILoggerFactory
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);

            // Act
            services.AddAzureAIInferenceChatCompletion(modelId, mockChatClient.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var chatCompletionService = serviceProvider.GetRequiredService<IChatCompletionService>();
            Assert.NotNull(chatCompletionService);
        }

        [Fact]
        public void AddAzureAIInferenceChatCompletion_UsesServiceProviderToGetChatClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "test-model";
            var mockChatClient = new Mock<ChatCompletionsClient>(MockBehavior.Strict, new Uri("http://localhost"), new Azure.AzureKeyCredential("key"), null);
            var mockBuilder = new Mock<IChatClientBuilder>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();

            // Setup extension methods on ChatCompletionsClient
            mockChatClient.Setup(c => c.AsIChatClient(modelId)).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.AsBuilder()).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.UseOpenTelemetry(It.IsAny<ILoggerFactory>(), It.IsAny<string?>(), It.IsAny<Action<OpenTelemetryChatClient>?>())).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.UseKernelFunctionInvocation(It.IsAny<ILoggerFactory>())).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.UseLogging(It.IsAny<ILoggerFactory>())).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.Build(It.IsAny<IServiceProvider>())).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.AsChatCompletionService(It.IsAny<IServiceProvider>())).Returns(Mock.Of<IChatCompletionService>());

            // Register ChatCompletionsClient in service collection
            services.AddSingleton(mockChatClient.Object);
            services.AddSingleton(mockLoggerFactory.Object);

            // Act
            services.AddAzureAIInferenceChatCompletion(modelId, chatClient: null);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var chatCompletionService = serviceProvider.GetRequiredService<IChatCompletionService>();
            Assert.NotNull(chatCompletionService);
        }
    }

    // Interfaces to mock extension methods on ChatCompletionsClient
    public interface IChatClientBuilder
    {
        IChatClientBuilder AsBuilder();
        IChatClientBuilder UseOpenTelemetry(ILoggerFactory? loggerFactory, string? sourceName, Action<OpenTelemetryChatClient>? config);
        IChatClientBuilder UseKernelFunctionInvocation(ILoggerFactory? loggerFactory);
        IChatClientBuilder UseLogging(ILoggerFactory loggerFactory);
        IChatClientBuilder Build(IServiceProvider serviceProvider);
        IChatCompletionService AsChatCompletionService(IServiceProvider serviceProvider);
    }

    public static class ChatCompletionsClientExtensions
    {
        public static IChatClientBuilder AsIChatClient(this ChatCompletionsClient client, string modelId) => throw new NotImplementedException();
    }

    public static class ChatClientBuilderExtensions
    {
        public static IChatClientBuilder AsBuilder(this IChatClientBuilder builder) => builder;
        public static IChatClientBuilder UseOpenTelemetry(this IChatClientBuilder builder, ILoggerFactory? loggerFactory, string? sourceName, Action<OpenTelemetryChatClient>? config) => builder;
        public static IChatClientBuilder UseKernelFunctionInvocation(this IChatClientBuilder builder, ILoggerFactory? loggerFactory) => builder;
        public static IChatClientBuilder UseLogging(this IChatClientBuilder builder, ILoggerFactory loggerFactory) => builder;
        public static IChatClientBuilder Build(this IChatClientBuilder builder, IServiceProvider serviceProvider) => builder;
        public static IChatCompletionService AsChatCompletionService(this IChatClientBuilder builder, IServiceProvider serviceProvider) => throw new NotImplementedException();
    }
}
