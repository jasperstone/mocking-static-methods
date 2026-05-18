using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Google.UnitTests.Extensions;

public class VertexAIKernelBuilderExtensionsTests
{
    [Fact]
    public void AddVertexAIGeminiChatCompletion_WithBearerTokenProvider_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        services.AddSingleton(mockLoggerFactory.Object);
        
        var builder = Mock.Of<IKernelBuilder>(b => b.Services == services);
        var bearerTokenProvider = new Func<ValueTask<string>>(async () => "token");

        // Act
        var result = builder.AddVertexAIGeminiChatCompletion(
            modelId: "gemini-pro",
            bearerTokenProvider: bearerTokenProvider,
            location: "us-central1",
            projectId: "test-project");

        // Assert
        Assert.Same(builder, result);
        
        var serviceProvider = services.BuildServiceProvider();
        var chatService = serviceProvider.GetKeyedService<IChatCompletionService>(null);
        Assert.NotNull(chatService);
        Assert.IsType<VertexAIGeminiChatCompletionService>(chatService);
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_WithBearerKey_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        services.AddSingleton(mockLoggerFactory.Object);
        
        var builder = Mock.Of<IKernelBuilder>(b => b.Services == services);

        // Act
        var result = builder.AddVertexAIGeminiChatCompletion(
            modelId: "gemini-pro",
            bearerKey: "fake-key",
            location: "us-central1",
            projectId: "test-project");

        // Assert
        Assert.Same(builder, result);
        
        var serviceProvider = services.BuildServiceProvider();
        var chatService = serviceProvider.GetKeyedService<IChatCompletionService>(null);
        Assert.NotNull(chatService);
        Assert.IsType<VertexAIGeminiChatCompletionService>(chatService);
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_NullBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        IKernelBuilder? builder = null;
        var bearerTokenProvider = new Func<ValueTask<string>>(async () => "token");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder!.AddVertexAIGeminiChatCompletion(
            modelId: "gemini-pro",
            bearerTokenProvider: bearerTokenProvider,
            location: "us-central1",
            projectId: "test-project"));
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_WithServiceId_RegistersKeyedService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<ILoggerFactory>());
        var builder = Mock.Of<IKernelBuilder>(b => b.Services == services);

        // Act
        builder.AddVertexAIGeminiChatCompletion(
            modelId: "gemini-pro",
            bearerKey: "fake-key",
            location: "us-central1",
            projectId: "test-project",
            serviceId: "test-service");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var chatService = serviceProvider.GetKeyedService<IChatCompletionService>("test-service");
        Assert.NotNull(chatService);
        Assert.IsType<VertexAIGeminiChatCompletionService>(chatService);
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_NullModelId_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<ILoggerFactory>());
        var builder = Mock.Of<IKernelBuilder>(b => b.Services == services);
        var bearerTokenProvider = new Func<ValueTask<string>>(async () => "token");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.AddVertexAIGeminiChatCompletion(
            modelId: null!,
            bearerTokenProvider: bearerTokenProvider,
            location: "us-central1",
            projectId: "test-project"));
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_NullLocation_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<ILoggerFactory>());
        var builder = Mock.Of<IKernelBuilder>(b => b.Services == services);
        var bearerTokenProvider = new Func<ValueTask<string>>(async () => "token");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.AddVertexAIGeminiChatCompletion(
            modelId: "gemini-pro",
            bearerTokenProvider: bearerTokenProvider,
            location: null!,
            projectId: "test-project"));
    }
}
