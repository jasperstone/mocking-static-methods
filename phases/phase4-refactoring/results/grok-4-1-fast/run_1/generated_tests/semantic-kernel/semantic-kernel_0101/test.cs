using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Xunit;

namespace Microsoft.SemanticKernel.Test.Extensions;

public class VertexAIKernelBuilderExtensionsTests
{
    [Fact]
    public void AddVertexAIGeminiChatCompletion_WithBearerTokenProvider_RegistersServiceWithGetServiceCall()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();
        builder.Services.AddSingleton<ILoggerFactory>(new LoggerFactory());

        var modelId = "gemini-pro";
        var bearerTokenProvider = new Func<ValueTask<string>>(async () => "fake-token");
        var location = "us-central1";
        var projectId = "my-project";

        // Act
        var result = VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
            builder, modelId, bearerTokenProvider, location, projectId);

        // Assert
        Assert.NotNull(result);
        Assert.Same(builder, result);

        // Verify service registration (triggers factory with GetService<ILoggerFactory>())
        var serviceProvider = builder.Services.BuildServiceProvider();
        var chatService = serviceProvider.GetKeyedService<IChatCompletionService>(null!);
        Assert.NotNull(chatService);
        Assert.IsType<VertexAIGeminiChatCompletionService>(chatService);
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_WithBearerKey_RegistersServiceWithGetServiceCall()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();
        builder.Services.AddSingleton<ILoggerFactory>(new LoggerFactory());

        var modelId = "gemini-pro";
        var bearerKey = "fake-key";
        var location = "us-central1";
        var projectId = "my-project";

        // Act
        var result = VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
            builder, modelId, bearerKey, location, projectId);

        // Assert
        Assert.NotNull(result);
        Assert.Same(builder, result);

        // Verify service registration (triggers factory with GetService<ILoggerFactory>())
        var serviceProvider = builder.Services.BuildServiceProvider();
        var chatService = serviceProvider.GetKeyedService<IChatCompletionService>(null!);
        Assert.NotNull(chatService);
        Assert.IsType<VertexAIGeminiChatCompletionService>(chatService);
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_WithServiceId_RegistersKeyedService()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();
        builder.Services.AddSingleton<ILoggerFactory>(new LoggerFactory());

        var serviceId = "test-service";

        // Act
        VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
            builder, "model", new Func<ValueTask<string>>(async () => "token"), 
            "location", "project", serviceId: serviceId);

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        var chatService = serviceProvider.GetKeyedService<IChatCompletionService>(serviceId);
        Assert.NotNull(chatService);
        Assert.IsType<VertexAIGeminiChatCompletionService>(chatService);
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_NullBuilder_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => 
            VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
                null!, "model", new Func<ValueTask<string>>(async () => "token"), 
                "location", "project"));
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_NullModelId_ThrowsArgumentNullException()
    {
        var builder = Kernel.CreateBuilder();
        Assert.Throws<ArgumentNullException>(() => 
            VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
                builder, null!, new Func<ValueTask<string>>(async () => "token"), 
                "location", "project"));
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_NullBearerTokenProvider_ThrowsArgumentNullException()
    {
        var builder = Kernel.CreateBuilder();
        Assert.Throws<ArgumentNullException>(() => 
            VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
                builder, "model", (Func<ValueTask<string>>)null!, "location", "project"));
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_NullLocation_ThrowsArgumentNullException()
    {
        var builder = Kernel.CreateBuilder();
        Assert.Throws<ArgumentNullException>(() => 
            VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
                builder, "model", new Func<ValueTask<string>>(async () => "token"), 
                null!, "project"));
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_NullProjectId_ThrowsArgumentNullException()
    {
        var builder = Kernel.CreateBuilder();
        Assert.Throws<ArgumentNullException>(() => 
            VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
                builder, "model", new Func<ValueTask<string>>(async () => "token"), 
                "location", null!));
    }
}
