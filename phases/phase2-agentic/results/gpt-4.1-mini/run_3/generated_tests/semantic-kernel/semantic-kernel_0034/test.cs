using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Azure.AI.Inference;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureAIInference.Core;

public class AzureAIInferenceServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAzureAIInferenceChatCompletion_UsesProvidedChatClient()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockChatClient = new Mock<ChatCompletionsClient>(MockBehavior.Strict, new Uri("http://localhost"), new Azure.AzureKeyCredential("key"), null);
        var mockBuilder = new Mock<IChatClientBuilder>();
        var mockChatClientInterface = new Mock<IChatClient>();

        // Setup fluent calls on ChatCompletionsClient extension methods
        mockChatClient.Setup(c => c.AsIChatClient(It.IsAny<string>())).Returns(mockChatClientInterface.Object);
        mockChatClientInterface.Setup(c => c.AsBuilder()).Returns(mockBuilder.Object);
        mockBuilder.Setup(b => b.UseOpenTelemetry(It.IsAny<ILoggerFactory>(), It.IsAny<string>(), It.IsAny<Action<OpenTelemetryChatClient>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(b => b.UseKernelFunctionInvocation(It.IsAny<ILoggerFactory>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(b => b.UseLogging(It.IsAny<ILoggerFactory>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(b => b.Build(It.IsAny<IServiceProvider>())).Returns(mockChatClientInterface.Object);
        mockChatClientInterface.Setup(c => c.AsChatCompletionService(It.IsAny<IServiceProvider>())).Returns(Mock.Of<IChatCompletionService>());

        var mockLoggerFactory = new Mock<ILoggerFactory>();

        // Add a logger factory to the service provider
        services.AddSingleton(mockLoggerFactory.Object);

        // Act
        var result = services.AddAzureAIInferenceChatCompletion(
            modelId: "test-model",
            chatClient: mockChatClient.Object,
            openTelemetrySourceName: "source",
            openTelemetryConfig: null);

        // Build service provider and resolve the service to trigger the factory
        var serviceProvider = services.BuildServiceProvider();
        var service = serviceProvider.GetRequiredService<IChatCompletionService>();

        // Assert
        Assert.Same(services, result);
        mockChatClient.Verify(c => c.AsIChatClient("test-model"), Times.Once);
        mockBuilder.Verify(b => b.UseOpenTelemetry(mockLoggerFactory.Object, "source", null), Times.Once);
        mockBuilder.Verify(b => b.UseKernelFunctionInvocation(mockLoggerFactory.Object), Times.Once);
        mockBuilder.Verify(b => b.UseLogging(mockLoggerFactory.Object), Times.Once);
        mockBuilder.Verify(b => b.Build(serviceProvider), Times.Once);
    }

    [Fact]
    public void AddAzureAIInferenceChatCompletion_UsesServiceProviderToGetChatClient()
    {
        // Arrange
        var services = new ServiceCollection();

        var mockChatClient = new Mock<ChatCompletionsClient>(MockBehavior.Strict, new Uri("http://localhost"), new Azure.AzureKeyCredential("key"), null);
        var mockBuilder = new Mock<IChatClientBuilder>();
        var mockChatClientInterface = new Mock<IChatClient>();

        // Setup fluent calls on ChatCompletionsClient extension methods
        mockChatClient.Setup(c => c.AsIChatClient(It.IsAny<string>())).Returns(mockChatClientInterface.Object);
        mockChatClientInterface.Setup(c => c.AsBuilder()).Returns(mockBuilder.Object);
        mockBuilder.Setup(b => b.UseOpenTelemetry(It.IsAny<ILoggerFactory>(), It.IsAny<string>(), It.IsAny<Action<OpenTelemetryChatClient>>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(b => b.UseKernelFunctionInvocation(It.IsAny<ILoggerFactory>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(b => b.UseLogging(It.IsAny<ILoggerFactory>())).Returns(mockBuilder.Object);
        mockBuilder.Setup(b => b.Build(It.IsAny<IServiceProvider>())).Returns(mockChatClientInterface.Object);
        mockChatClientInterface.Setup(c => c.AsChatCompletionService(It.IsAny<IServiceProvider>())).Returns(Mock.Of<IChatCompletionService>());

        var mockLoggerFactory = new Mock<ILoggerFactory>();

        // Add the ChatCompletionsClient and LoggerFactory to the service provider
        services.AddSingleton(mockChatClient.Object);
        services.AddSingleton(mockLoggerFactory.Object);

        // Act
        var result = services.AddAzureAIInferenceChatCompletion(
            modelId: "test-model",
            chatClient: null,
            openTelemetrySourceName: "source",
            openTelemetryConfig: null);

        var serviceProvider = services.BuildServiceProvider();

        // The factory will call GetRequiredService<ChatCompletionsClient>() on serviceProvider
        var service = serviceProvider.GetRequiredService<IChatCompletionService>();

        // Assert
        Assert.Same(services, result);
        mockChatClient.Verify(c => c.AsIChatClient("test-model"), Times.Once);
        mockBuilder.Verify(b => b.UseOpenTelemetry(mockLoggerFactory.Object, "source", null), Times.Once);
        mockBuilder.Verify(b => b.UseKernelFunctionInvocation(mockLoggerFactory.Object), Times.Once);
        mockBuilder.Verify(b => b.UseLogging(mockLoggerFactory.Object), Times.Once);
        mockBuilder.Verify(b => b.Build(serviceProvider), Times.Once);
    }
}
