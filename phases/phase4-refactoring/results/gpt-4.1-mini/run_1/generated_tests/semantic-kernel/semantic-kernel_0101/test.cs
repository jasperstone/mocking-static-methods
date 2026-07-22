using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Xunit;

public class VertexAIKernelBuilderExtensionsTests
{
    private class TestKernelBuilderPlugins : IKernelBuilderPlugins
    {
        public IServiceCollection Services { get; } = new ServiceCollection();
    }

    private class TestKernelBuilder : IKernelBuilder
    {
        public IServiceCollection Services { get; } = new ServiceCollection();
        public IKernelBuilderPlugins Plugins { get; } = new TestKernelBuilderPlugins();
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_WithBearerTokenProvider_RegistersServiceAndCallsGetService()
    {
        // Arrange
        var builder = new TestKernelBuilder();
        string modelId = "test-model";
        Func<ValueTask<string>> bearerTokenProvider = () => new ValueTask<string>("token");
        string location = "us-central1";
        string projectId = "test-project";

        // Register ILoggerFactory in the service collection to be resolved by GetService
        builder.Services.AddSingleton<ILoggerFactory>(LoggerFactory.Create(builder => builder.AddConsole()));

        // Act
        var returnedBuilder = VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
            builder,
            modelId,
            bearerTokenProvider,
            location,
            projectId);

        // Assert
        Assert.Same(builder, returnedBuilder);

        // Verify that the service was registered and can be resolved
        var serviceProvider = builder.Services.BuildServiceProvider();
        var chatCompletionService = serviceProvider.GetService<IChatCompletionService>();
        Assert.NotNull(chatCompletionService);
        Assert.IsType<VertexAIGeminiChatCompletionService>(chatCompletionService);
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_WithBearerKey_RegistersServiceAndCallsGetService()
    {
        // Arrange
        var builder = new TestKernelBuilder();
        string modelId = "test-model";
        string bearerKey = "test-bearer-key";
        string location = "us-central1";
        string projectId = "test-project";

        // Register ILoggerFactory in the service collection to be resolved by GetService
        builder.Services.AddSingleton<ILoggerFactory>(LoggerFactory.Create(builder => builder.AddConsole()));

        // Act
        var returnedBuilder = VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
            builder,
            modelId,
            bearerKey,
            location,
            projectId);

        // Assert
        Assert.Same(builder, returnedBuilder);

        // Verify that the service was registered and can be resolved
        var serviceProvider = builder.Services.BuildServiceProvider();
        var chatCompletionService = serviceProvider.GetService<IChatCompletionService>();
        Assert.NotNull(chatCompletionService);
        Assert.IsType<VertexAIGeminiChatCompletionService>(chatCompletionService);
    }
}
