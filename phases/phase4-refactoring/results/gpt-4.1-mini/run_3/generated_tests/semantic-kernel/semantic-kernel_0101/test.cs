using System;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
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

        var loggerFactory = LoggerFactory.Create(lb => lb.AddConsole());
        builder.Services.AddSingleton(loggerFactory);

        // Act
        var returnedBuilder = VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
            builder,
            modelId,
            bearerTokenProvider,
            location,
            projectId);

        // Assert
        Assert.Same(builder, returnedBuilder);

        var serviceProvider = builder.Services.BuildServiceProvider();

        // The service should be registered keyed by null serviceId
        var chatCompletionService = serviceProvider.GetService<IChatCompletionService>();
        Assert.NotNull(chatCompletionService);

        // The logger factory should be resolved via GetService call on IServiceProvider
        var resolvedLoggerFactory = serviceProvider.GetService<ILoggerFactory>();
        Assert.NotNull(resolvedLoggerFactory);
        Assert.Same(loggerFactory, resolvedLoggerFactory);
    }
}
