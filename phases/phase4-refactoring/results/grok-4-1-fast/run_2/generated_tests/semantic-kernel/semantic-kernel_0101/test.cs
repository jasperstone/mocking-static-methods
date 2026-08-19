using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Google.UnitTests.Extensions;

public class VertexAIKernelBuilderExtensionsTests
{
    [Fact]
    public void AddVertexAIGeminiChatCompletion_WithBearerTokenProvider_RegistersService()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();
        builder.Services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        var modelId = "gemini-pro";
        var bearerTokenProvider = () => new ValueTask<string>("fake-token");
        var location = "us-central1";
        var projectId = "test-project";

        // Act
        var result = VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
            builder, modelId, bearerTokenProvider, location, projectId);

        // Assert
        Assert.NotNull(result);
        Assert.Same(builder, result);

        var serviceProvider = builder.Services.BuildServiceProvider();
        var chatService = serviceProvider.GetKeyedService<IChatCompletionService>(null);
        Assert.NotNull(chatService);
        Assert.IsType<VertexAIGeminiChatCompletionService>(chatService);
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_WithBearerKey_RegistersService()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();
        builder.Services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        var modelId = "gemini-pro";
        var bearerKey = "fake-key";
        var location = "us-central1";
        var projectId = "test-project";

        // Act
        var result = VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
            builder, modelId, bearerKey, location, projectId);

        // Assert
        Assert.NotNull(result);
        Assert.Same(builder, result);

        var serviceProvider = builder.Services.BuildServiceProvider();
        var chatService = serviceProvider.GetKeyedService<IChatCompletionService>(null);
        Assert.NotNull(chatService);
        Assert.IsType<VertexAIGeminiChatCompletionService>(chatService);
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_NullBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        var bearerTokenProvider = () => new ValueTask<string>("token");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
            builder: null!,
            modelId: "gemini-pro",
            bearerTokenProvider: bearerTokenProvider,
            location: "us-central1",
            projectId: "test-project"));
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_NullModelId_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();
        var bearerTokenProvider = () => new ValueTask<string>("token");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
            builder, modelId: null!, bearerTokenProvider, "us-central1", "test-project"));
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_WithServiceId_RegistersKeyedService()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();
        builder.Services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        var serviceId = "test-service";

        // Act
        VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
            builder,
            modelId: "gemini-pro",
            bearerTokenProvider: () => new ValueTask<string>("token"),
            location: "us-central1",
            projectId: "test-project",
            serviceId: serviceId);

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        var chatService = serviceProvider.GetKeyedService<IChatCompletionService>(serviceId);
        Assert.NotNull(chatService);
        Assert.IsType<VertexAIGeminiChatCompletionService>(chatService);
    }
}
