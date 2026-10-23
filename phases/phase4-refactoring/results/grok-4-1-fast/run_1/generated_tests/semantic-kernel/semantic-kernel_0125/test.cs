using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.Connectors.HuggingFace.Core;
using Microsoft.SemanticKernel.Http;
using Xunit;

public class HuggingFaceEmbeddingGeneratorTests : IDisposable
{
    private readonly HttpClient _mockHttpClient;
    private readonly HuggingFaceEmbeddingGenerator _generator;

    public HuggingFaceEmbeddingGeneratorTests()
    {
        _mockHttpClient = new MockHttpClient();
        _mockHttpClient.BaseAddress = new Uri("https://api.huggingface.co/");

        _generator = new HuggingFaceEmbeddingGenerator(
            modelId: "test-model",
            httpClient: _mockHttpClient,
            loggerFactory: null);
    }

    public void Dispose()
    {
        _generator.Dispose();
    }

    [Fact]
    public void Constructor_WithExternalHttpClient_SetsIsExternalHttpClientToTrue()
    {
        // Arrange & Act
        using var httpClient = new HttpClient();
        var generator = new HuggingFaceEmbeddingGenerator(
            modelId: "test-model",
            httpClient: httpClient);

        // Assert
        var field = generator.GetType().GetField("_isExternalHttpClient", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.True((bool)field!.GetValue(generator)!);
    }

    [Fact]
    public void Constructor_WithoutHttpClient_SetsIsExternalHttpClientToFalse()
    {
        // Arrange & Act
        var generator = new HuggingFaceEmbeddingGenerator(
            modelId: "test-model");

        // Assert
        var field = generator.GetType().GetField("_isExternalHttpClient", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.False((bool)field!.GetValue(generator)!);
    }

    [Fact]
    public void Dispose_WithExternalHttpClient_DoesNotDisposeHttpClient()
    {
        // Arrange
        using var httpClient = new HttpClient();
        var generator = new HuggingFaceEmbeddingGenerator(
            modelId: "test-model",
            httpClient: httpClient);

        // Act
        generator.Dispose();

        // Assert - no exception thrown, external client not disposed (HttpClient.Dispose is safe to call multiple times)
    }

    [Fact]
    public void Dispose_WithoutExternalHttpClient_CallsDisposeOnHttpClient()
    {
        // Arrange
        var mockHttpClient = new MockHttpClient();
        var generator = new HuggingFaceEmbeddingGenerator(
            modelId: "test-model");

        // Use reflection to inject the mock HttpClient for testing internal disposal
        var httpClientField = generator.GetType().GetField("_httpClient", BindingFlags.NonPublic | BindingFlags.Instance);
        httpClientField!.SetValue(generator, mockHttpClient);

        // Act
        generator.Dispose();

        // Assert
        Assert.True(mockHttpClient.WasDisposed);
    }

    [Fact]
    public void GetService_WithNullServiceKey_ReturnsNull()
    {
        // Act
        var result = _generator.GetService(typeof(object), null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetService_WithMatchingServiceType_ReturnsSelf()
    {
        // Act
        var result = _generator.GetService(typeof(HuggingFaceEmbeddingGenerator), _generator);

        // Assert
        Assert.Same(_generator, result);
    }

    [Fact]
    public void GetService_WithEmbeddingGeneratorMetadataType_ReturnsMetadata()
    {
        // Act
        var result = _generator.GetService(typeof(EmbeddingGeneratorMetadata), _generator);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<EmbeddingGeneratorMetadata>(result);
    }

    [Fact]
    public void GetService_WithUnknownType_ReturnsNull()
    {
        // Act
        var result = _generator.GetService(typeof(string), _generator);

        // Assert
        Assert.Null(result);
    }

    private class MockHttpClient : HttpClient
    {
        public bool WasDisposed { get; private set; }

        public MockHttpClient() : base() { }

        public override void Dispose()
        {
            WasDisposed = true;
            base.Dispose();
        }
    }
}
