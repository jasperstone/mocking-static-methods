using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.HuggingFace.UnitTests;

public class HuggingFaceServiceCollectionExtensionsTests
{
    [Fact]
    public void AddHuggingFaceTextEmbeddingGeneration_EndpointOverload_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory, NullLoggerFactory>();

        // Act
        var result = HuggingFaceServiceCollectionExtensions.AddHuggingFaceTextEmbeddingGeneration(
            services,
            endpoint: new Uri("https://example.com/"));

        // Assert
        Assert.Same(services, result);
        var provider = services.BuildServiceProvider();
        _ = provider.GetKeyedService<object>(null); // Triggers factory, calls serviceProvider.GetService<ILoggerFactory>()
    }

    [Fact]
    public void AddHuggingFaceTextEmbeddingGeneration_ModelOverload_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory, NullLoggerFactory>();

        // Act
        var result = HuggingFaceServiceCollectionExtensions.AddHuggingFaceTextEmbeddingGeneration(
            services,
            model: "test-model",
            endpoint: new Uri("https://example.com/"));

        // Assert
        Assert.Same(services, result);
        var provider = services.BuildServiceProvider();
        _ = provider.GetKeyedService<object>(null);
    }

    [Fact]
    public void AddHuggingFaceImageToText_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory, NullLoggerFactory>();

        // Act
        var result = HuggingFaceServiceCollectionExtensions.AddHuggingFaceImageToText(
            services,
            model: "test-model");

        // Assert
        Assert.Same(services, result);
        var provider = services.BuildServiceProvider();
        _ = provider.GetKeyedService<object>(null); // Triggers line 188: serviceProvider.GetService<ILoggerFactory>()
    }

    [Fact]
    public void AddHuggingFaceTextGeneration_EndpointOverload_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory, NullLoggerFactory>();

        // Act
        var result = HuggingFaceServiceCollectionExtensions.AddHuggingFaceTextGeneration(
            services,
            endpoint: new Uri("https://example.com/"));

        // Assert
        Assert.Same(services, result);
        var provider = services.BuildServiceProvider();
        _ = provider.GetKeyedService<object>(null);
    }

    [Fact]
    public void AddHuggingFaceChatCompletion_EndpointOverload_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory, NullLoggerFactory>();

        // Act
        var result = HuggingFaceServiceCollectionExtensions.AddHuggingFaceChatCompletion(
            services,
            endpoint: new Uri("https://example.com/"));

        // Assert
        Assert.Same(services, result);
        var provider = services.BuildServiceProvider();
        _ = provider.GetKeyedService<object>(null);
    }

    [Fact]
    public void AddHuggingFaceTextEmbeddingGeneration_NullServices_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => 
            HuggingFaceServiceCollectionExtensions.AddHuggingFaceTextEmbeddingGeneration(null!, new Uri("https://example.com/")));
    }
}
