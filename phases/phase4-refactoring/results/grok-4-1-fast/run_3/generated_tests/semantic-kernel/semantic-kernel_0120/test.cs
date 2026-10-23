using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Http;
using Microsoft.SemanticKernel.ImageToText;
using Microsoft.SemanticKernel.TextGeneration;
using Xunit;

namespace Microsoft.SemanticKernel.Test;

public class HuggingFaceServiceCollectionExtensionsTests
{
    [Fact]
    public void AddHuggingFaceTextEmbeddingGeneration_EndpointOverload_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var expectedLoggerFactory = NullLoggerFactory.Instance;
        services.TryAddSingleton<ILoggerFactory>(expectedLoggerFactory);

        // Act
        var result = HuggingFaceServiceCollectionExtensions.AddHuggingFaceTextEmbeddingGeneration(
            services,
            new Uri("https://example.com/endpoint"));

        // Assert
        Assert.Same(services, result);
        Assert.Single(services);

        using var sp = result.BuildServiceProvider();
        var service = sp.GetKeyedService<ITextEmbeddingGenerationService>(null!);
        Assert.NotNull(service);
    }

    [Fact]
    public void AddHuggingFaceImageToText_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var expectedLoggerFactory = NullLoggerFactory.Instance;
        services.TryAddSingleton<ILoggerFactory>(expectedLoggerFactory);

        // Act
        var result = HuggingFaceServiceCollectionExtensions.AddHuggingFaceImageToText(
            services,
            model: "test-model");

        // Assert
        Assert.Same(services, result);
        Assert.Single(services);

        using var sp = result.BuildServiceProvider();
        var service = sp.GetKeyedService<IImageToTextService>(null!);
        Assert.NotNull(service);
    }
}
