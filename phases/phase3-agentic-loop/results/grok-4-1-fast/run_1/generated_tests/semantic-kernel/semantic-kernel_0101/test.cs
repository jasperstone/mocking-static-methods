using System;
using System.Collections.Generic;
using System.Linq;
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
    public void AddVertexAIGeminiChatCompletion_WithBearerTokenProvider_RegistersServiceWithFactoryCallingGetService()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        services.AddSingleton(mockLoggerFactory.Object);
        var builder = new MockKernelBuilder(services);

        var modelId = "gemini-pro";
        var bearerTokenProvider = () => new ValueTask<string>("fake-token");
        var location = "us-central1";
        var projectId = "my-project";

        // Act
        var result = builder.AddVertexAIGeminiChatCompletion(
            modelId,
            bearerTokenProvider,
            location,
            projectId);

        // Assert
        Assert.Same(builder, result);
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IChatCompletionService));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        Assert.NotNull(descriptor.ImplementationFactory);

        // Verify factory calls GetService<ILoggerFactory> by successfully creating the service
        var serviceProvider = services.BuildServiceProvider();
        var chatService = serviceProvider.GetRequiredService<IChatCompletionService>();
        Assert.IsType<VertexAIGeminiChatCompletionService>(chatService);
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_WithBearerKey_RegistersServiceWithFactoryCallingGetService()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        services.AddSingleton(mockLoggerFactory.Object);
        var builder = new MockKernelBuilder(services);

        var modelId = "gemini-pro";
        var bearerKey = "fake-key";
        var location = "us-central1";
        var projectId = "my-project";

        // Act
        var result = builder.AddVertexAIGeminiChatCompletion(
            modelId,
            bearerKey,
            location,
            projectId);

        // Assert
        Assert.Same(builder, result);
        var serviceProvider = services.BuildServiceProvider();
        var chatService = serviceProvider.GetRequiredService<IChatCompletionService>();
        Assert.IsType<VertexAIGeminiChatCompletionService>(chatService);
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_NullBuilder_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            ((IKernelBuilder?)null)!.AddVertexAIGeminiChatCompletion("model", () => new ValueTask<string>("token"), "loc", "proj"));
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_NullModelId_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        services.AddSingleton(mockLoggerFactory.Object);
        var builder = new MockKernelBuilder(services);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            builder.AddVertexAIGeminiChatCompletion(null!, () => new ValueTask<string>("token"), "loc", "proj"));
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_NullBearerTokenProvider_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        services.AddSingleton(mockLoggerFactory.Object);
        var builder = new MockKernelBuilder(services);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            builder.AddVertexAIGeminiChatCompletion("model", (Func<ValueTask<string>>?)null!, "loc", "proj"));
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_NullBearerKey_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        services.AddSingleton(mockLoggerFactory.Object);
        var builder = new MockKernelBuilder(services);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            builder.AddVertexAIGeminiChatCompletion("model", (string)null!, "loc", "proj"));
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_NullLocation_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        services.AddSingleton(mockLoggerFactory.Object);
        var builder = new MockKernelBuilder(services);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            builder.AddVertexAIGeminiChatCompletion("model", "key", null!, "proj"));
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_NullProjectId_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        services.AddSingleton(mockLoggerFactory.Object);
        var builder = new MockKernelBuilder(services);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            builder.AddVertexAIGeminiChatCompletion("model", "key", "loc", null!));
    }

    private class MockKernelBuilder : IKernelBuilder
    {
        public IServiceCollection Services { get; }
        public IKernelBuilderPlugins Plugins => new MockKernelBuilderPlugins(Services);

        public MockKernelBuilder(IServiceCollection services)
        {
            Services = services;
        }
    }

    private class MockKernelBuilderPlugins : IKernelBuilderPlugins
    {
        public IServiceCollection Services { get; }

        public MockKernelBuilderPlugins(IServiceCollection services)
        {
            Services = services;
        }
    }
}
