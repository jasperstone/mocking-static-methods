using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Xunit;

namespace Microsoft.SemanticKernel.Test;

public class VertexAIKernelBuilderExtensionsTests
{
    [Fact]
    public void AddVertexAIGeminiChatCompletion_WithBearerTokenProvider_RegistersKeyedSingleton()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();
        builder.Services.AddLogging();

        // Act
        var result = builder.AddVertexAIGeminiChatCompletion(
            modelId: "gemini-pro",
            bearerTokenProvider: () => new ValueTask<string>("fake-token"),
            location: "us-central1",
            projectId: "test-project");

        // Assert
        Assert.Same(builder, result);
        var serviceProvider = builder.Services.BuildServiceProvider();
        var chatService = serviceProvider.GetRequiredKeyedService<IChatCompletionService>(null!);
        Assert.NotNull(chatService);
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_WithBearerKey_RegistersKeyedSingleton()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();
        builder.Services.AddLogging();

        // Act
        var result = builder.AddVertexAIGeminiChatCompletion(
            modelId: "gemini-pro",
            bearerKey: "fake-key",
            location: "us-central1",
            projectId: "test-project");

        // Assert
        Assert.Same(builder, result);
        var serviceProvider = builder.Services.BuildServiceProvider();
        var chatService = serviceProvider.GetRequiredKeyedService<IChatCompletionService>(null!);
        Assert.NotNull(chatService);
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_NullBuilder_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ((IKernelBuilder?)null!).AddVertexAIGeminiChatCompletion(
            modelId: "gemini-pro",
            bearerTokenProvider: () => new ValueTask<string>("token"),
            location: "us-central1",
            projectId: "test-project"));
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_NullModelId_ThrowsArgumentNullException()
    {
        var builder = Kernel.CreateBuilder();
        Assert.Throws<ArgumentNullException>(() => builder.AddVertexAIGeminiChatCompletion(
            modelId: null!,
            bearerTokenProvider: () => new ValueTask<string>("token"),
            location: "us-central1",
            projectId: "test-project"));
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_NullBearerTokenProvider_ThrowsArgumentNullException()
    {
        var builder = Kernel.CreateBuilder();
        Assert.Throws<ArgumentNullException>(() => builder.AddVertexAIGeminiChatCompletion(
            modelId: "gemini-pro",
            bearerTokenProvider: null!,
            location: "us-central1",
            projectId: "test-project"));
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_NullLocation_ThrowsArgumentNullException()
    {
        var builder = Kernel.CreateBuilder();
        Assert.Throws<ArgumentNullException>(() => builder.AddVertexAIGeminiChatCompletion(
            modelId: "gemini-pro",
            bearerTokenProvider: () => new ValueTask<string>("token"),
            location: null!,
            projectId: "test-project"));
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_NullProjectId_ThrowsArgumentNullException()
    {
        var builder = Kernel.CreateBuilder();
        Assert.Throws<ArgumentNullException>(() => builder.AddVertexAIGeminiChatCompletion(
            modelId: "gemini-pro",
            bearerTokenProvider: () => new ValueTask<string>("token"),
            location: "us-central1",
            projectId: null!));
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_WithBearerKey_NullBearerKey_ThrowsArgumentNullException()
    {
        var builder = Kernel.CreateBuilder();
        Assert.Throws<ArgumentNullException>(() => builder.AddVertexAIGeminiChatCompletion(
            modelId: "gemini-pro",
            bearerKey: null!,
            location: "us-central1",
            projectId: "test-project"));
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_WithServiceId_RegistersKeyedService()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();
        builder.Services.AddLogging();

        // Act
        builder.AddVertexAIGeminiChatCompletion(
            modelId: "gemini-pro",
            bearerTokenProvider: () => new ValueTask<string>("token"),
            location: "us-central1",
            projectId: "test-project",
            serviceId: "test-service");

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        var chatService = serviceProvider.GetRequiredKeyedService<IChatCompletionService>("test-service");
        Assert.NotNull(chatService);
    }
}
