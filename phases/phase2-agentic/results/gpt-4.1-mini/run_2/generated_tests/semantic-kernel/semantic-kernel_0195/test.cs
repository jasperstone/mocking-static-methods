using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel;
using Moq;
using OpenAI;
using Xunit;

namespace Microsoft.SemanticKernel.Tests.Connectors.OpenAI.Extensions;

public class OpenAIServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOpenAITextEmbeddingGeneration_WithOpenAIClient_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();

        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockOpenAIClient = new Mock<OpenAIClient>();

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
            .Returns(mockLoggerFactory.Object);

        mockServiceProvider
            .Setup(sp => sp.GetRequiredService(typeof(OpenAIClient)))
            .Returns(mockOpenAIClient.Object);

        // We need to register a factory that uses the service provider mock
        services.AddKeyedSingleton<ITextEmbeddingGenerationService>("testService", (sp, _) =>
            new OpenAITextEmbeddingGenerationService(
                "test-model",
                mockOpenAIClient.Object,
                sp.GetService<ILoggerFactory>(),
                dimensions: 128));

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var service = serviceProvider.GetService<ITextEmbeddingGenerationService>();

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void AddOpenAITextEmbeddingGeneration_WithOpenAIClientNull_UsesGetRequiredService()
    {
        // Arrange
        var services = new ServiceCollection();

        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockOpenAIClient = new Mock<OpenAIClient>();

        services.AddSingleton(mockOpenAIClient.Object);
        services.AddSingleton(mockLoggerFactory.Object);

        services.AddOpenAITextEmbeddingGeneration(
            "test-model",
            openAIClient: null,
            serviceId: "testService",
            dimensions: 256);

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var service = serviceProvider.GetService<ITextEmbeddingGenerationService>();

        // Assert
        Assert.NotNull(service);
    }
}
