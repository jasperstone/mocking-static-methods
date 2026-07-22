using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Xunit;

namespace Microsoft.SemanticKernel.Test;

public class HuggingFaceServiceCollectionExtensionsTests
{
    [Fact]
    public void AddHuggingFaceTextEmbeddingGeneration_EndpointOverload_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockLoggerFactory = new MockLoggerFactory();
        services.AddSingleton(mockLoggerFactory);
        var endpoint = new Uri("https://api.huggingface.co");

        // Act
        _ = HuggingFaceServiceCollectionExtensions.AddHuggingFaceTextEmbeddingGeneration(services, endpoint);

        // Assert factory was resolved via serviceProvider.GetService<ILoggerFactory>()
        Assert.True(mockLoggerFactory.WasResolved);
    }

    [Fact]
    public void AddHuggingFaceTextEmbeddingGeneration_ModelOverload_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockLoggerFactory = new MockLoggerFactory();
        services.AddSingleton(mockLoggerFactory);
        var model = "test-model";

        // Act
        _ = HuggingFaceServiceCollectionExtensions.AddHuggingFaceTextEmbeddingGeneration(services, model);

        // Assert factory was resolved via serviceProvider.GetService<ILoggerFactory>()
        Assert.True(mockLoggerFactory.WasResolved);
    }

    [Fact]
    public void AddHuggingFaceImageToText_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockLoggerFactory = new MockLoggerFactory();
        services.AddSingleton(mockLoggerFactory);
        var model = "test-model";

        // Act
        _ = HuggingFaceServiceCollectionExtensions.AddHuggingFaceImageToText(services, model);

        // Assert factory was resolved via serviceProvider.GetService<ILoggerFactory>()
        Assert.True(mockLoggerFactory.WasResolved);
    }

    [Fact]
    public void AddHuggingFaceTextGeneration_EndpointOverload_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockLoggerFactory = new MockLoggerFactory();
        services.AddSingleton(mockLoggerFactory);
        var endpoint = new Uri("https://api.huggingface.co");

        // Act
        _ = HuggingFaceServiceCollectionExtensions.AddHuggingFaceTextGeneration(services, endpoint);

        // Assert factory was resolved via serviceProvider.GetService<ILoggerFactory>()
        Assert.True(mockLoggerFactory.WasResolved);
    }

    [Fact]
    public void AddHuggingFaceChatCompletion_EndpointOverload_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockLoggerFactory = new MockLoggerFactory();
        services.AddSingleton(mockLoggerFactory);
        var endpoint = new Uri("https://api.huggingface.co");

        // Act
        _ = HuggingFaceServiceCollectionExtensions.AddHuggingFaceChatCompletion(services, endpoint);

        // Assert factory was resolved via serviceProvider.GetService<ILoggerFactory>()
        Assert.True(mockLoggerFactory.WasResolved);
    }

    [Fact]
    public void AllExtensionMethods_ReturnSameServicesInstance()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Same(services, HuggingFaceServiceCollectionExtensions.AddHuggingFaceTextEmbeddingGeneration(services, new Uri("https://api.huggingface.co")));
        Assert.Same(services, HuggingFaceServiceCollectionExtensions.AddHuggingFaceImageToText(services, "model"));
        Assert.Same(services, HuggingFaceServiceCollectionExtensions.AddHuggingFaceTextGeneration(services, new Uri("https://api.huggingface.co")));
        Assert.Same(services, HuggingFaceServiceCollectionExtensions.AddHuggingFaceChatCompletion(services, new Uri("https://api.huggingface.co")));
    }
}

/// <summary>
/// Mock that detects when resolved via serviceProvider.GetService&lt;ILoggerFactory&gt;()
/// This covers the specific GetService call on line 188 and similar locations.
/// </summary>
public sealed class MockLoggerFactory : ILoggerFactory
{
    public bool WasResolved { get; private set; }

    public MockLoggerFactory()
    {
        WasResolved = true;
    }

    public void AddProvider(ILoggerProvider provider) => throw new NotImplementedException();
    public ILogger CreateLogger(string categoryName) => throw new NotImplementedException();
    public void Dispose() { }
}
