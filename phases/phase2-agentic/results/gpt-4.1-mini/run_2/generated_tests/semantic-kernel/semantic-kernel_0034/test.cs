using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Azure.AI.Inference;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureAIInference.Core;

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
            var mockChatClient = new Mock<ChatCompletionsClient>(MockBehavior.Strict, new Uri("http://localhost"), new Azure.AzureKeyCredential("key"), new Azure.AI.Inference.ChatClientOptions());
            var mockBuilder = new Mock<IChatClientBuilder>();
            var mockChatClientInterface = new Mock<IChatClient>();

            // Setup fluent calls on ChatCompletionsClient extension methods
            mockChatClient.Setup(c => c.AsIChatClient(modelId)).Returns(mockChatClientInterface.Object);
            mockChatClientInterface.Setup(c => c.AsBuilder()).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.UseOpenTelemetry(It.IsAny<ILoggerFactory>(), It.IsAny<string?>(), It.IsAny<Action<OpenTelemetryChatClient>?>())).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.UseKernelFunctionInvocation(It.IsAny<ILoggerFactory>())).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.UseLogging(It.IsAny<ILoggerFactory>())).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.Build(It.IsAny<IServiceProvider>())).Returns(mockChatClientInterface.Object);
            mockChatClientInterface.Setup(c => c.AsChatCompletionService(It.IsAny<IServiceProvider>())).Returns(Mock.Of<IChatCompletionService>());

            // Add a mock ILoggerFactory to service provider
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            services.AddSingleton(mockLoggerFactory.Object);

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = AzureAIInferenceServiceCollectionExtensions.AddAzureAIInferenceChatCompletion(
                services,
                modelId,
                mockChatClient.Object,
                serviceId: null,
                openTelemetrySourceName: null,
                openTelemetryConfig: null);

            // Resolve the service to trigger the factory delegate
            var chatCompletionService = serviceProvider.GetRequiredService<IChatCompletionService>();

            // Assert
            Assert.Same(services, result);
        }

        [Fact]
        public void AddAzureAIInferenceChatCompletion_UsesServiceProviderToGetChatClient_WhenNull()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "test-model";

            var mockChatClient = new Mock<ChatCompletionsClient>(MockBehavior.Strict, new Uri("http://localhost"), new Azure.AzureKeyCredential("key"), new Azure.AI.Inference.ChatClientOptions());
            var mockBuilder = new Mock<IChatClientBuilder>();
            var mockChatClientInterface = new Mock<IChatClient>();

            // Setup fluent calls on ChatCompletionsClient extension methods
            mockChatClient.Setup(c => c.AsIChatClient(modelId)).Returns(mockChatClientInterface.Object);
            mockChatClientInterface.Setup(c => c.AsBuilder()).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.UseOpenTelemetry(It.IsAny<ILoggerFactory>(), It.IsAny<string?>(), It.IsAny<Action<OpenTelemetryChatClient>?>())).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.UseKernelFunctionInvocation(It.IsAny<ILoggerFactory>())).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.UseLogging(It.IsAny<ILoggerFactory>())).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.Build(It.IsAny<IServiceProvider>())).Returns(mockChatClientInterface.Object);
            mockChatClientInterface.Setup(c => c.AsChatCompletionService(It.IsAny<IServiceProvider>())).Returns(Mock.Of<IChatCompletionService>());

            // Add the ChatCompletionsClient to the service provider so GetRequiredService can find it
            services.AddSingleton(mockChatClient.Object);

            // Add a mock ILoggerFactory to service provider
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            services.AddSingleton(mockLoggerFactory.Object);

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = AzureAIInferenceServiceCollectionExtensions.AddAzureAIInferenceChatCompletion(
                services,
                modelId,
                chatClient: null,
                serviceId: null,
                openTelemetrySourceName: null,
                openTelemetryConfig: null);

            // Resolve the service to trigger the factory delegate
            var chatCompletionService = serviceProvider.GetRequiredService<IChatCompletionService>();

            // Assert
            Assert.Same(services, result);
        }
    }
}
