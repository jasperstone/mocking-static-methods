using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Moq;
using Moq.Protected;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.HuggingFace.UnitTests;

public class HuggingFaceEmbeddingGeneratorTests
{
    private static readonly Uri TestEndpoint = new("https://test-endpoint.com");

    [Fact]
    public void Constructor_WithModelIdAndNoHttpClient_SetsIsExternalHttpClientToFalse()
    {
        // Act
        var generator = new HuggingFaceEmbeddingGenerator("test-model");

        // Assert
        Assert.False(GetIsExternalHttpClient(generator));
    }

    [Fact]
    public void Constructor_WithModelIdAndHttpClient_SetsIsExternalHttpClientToTrue()
    {
        // Arrange
        using var httpClient = new HttpClient();

        // Act
        var generator = new HuggingFaceEmbeddingGenerator(
            modelId: "test-model",
            httpClient: httpClient);

        // Assert
        Assert.True(GetIsExternalHttpClient(generator));
    }

    [Fact]
    public void Constructor_WithEndpointAndNoHttpClient_SetsIsExternalHttpClientToFalse()
    {
        // Act
        var generator = new HuggingFaceEmbeddingGenerator(TestEndpoint);

        // Assert
        Assert.False(GetIsExternalHttpClient(generator));
    }

    [Fact]
    public void Dispose_WithInternalHttpClient_CallsHttpClientDispose()
    {
        // Arrange - Create generator with internal HttpClient (no httpClient passed)
        var generator = new HuggingFaceEmbeddingGenerator(TestEndpoint);

        // Use reflection to get the internal HttpClient field
        var httpClientField = typeof(HuggingFaceEmbeddingGenerator)
            .GetField("_httpClient", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var httpClient = (HttpClient)httpClientField.GetValue(generator)!;

        // Verify HttpClient was not disposed before Dispose call
        Assert.False(httpClient.DisposeHasBeenCalled());

        // Act
        generator.Dispose();

        // Assert - HttpClient should now be disposed
        Assert.True(httpClient.DisposeHasBeenCalled());
    }

    [Fact]
    public void Dispose_WithExternalHttpClient_DoesNotCallHttpClientDispose()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Loose);
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

        using var httpClient = new HttpClient(mockHandler.Object);
        var generator = new HuggingFaceEmbeddingGenerator(
            endpoint: TestEndpoint,
            httpClient: httpClient);

        // Verify HttpClient was not disposed before Dispose call
        Assert.False(httpClient.DisposeHasBeenCalled());

        // Act
        generator.Dispose();

        // Assert - External HttpClient should NOT be disposed
        Assert.False(httpClient.DisposeHasBeenCalled());
    }

    [Fact]
    public void GetService_WithEmbeddingGeneratorMetadataType_ReturnsMetadata()
    {
        // Arrange
        var generator = new HuggingFaceEmbeddingGenerator("test-model");

        // Act
        var metadata = generator.GetService(typeof(EmbeddingGeneratorMetadata));

        // Assert
        Assert.NotNull(metadata);
        Assert.IsType<EmbeddingGeneratorMetadata>(metadata);
    }

    [Fact]
    public void GetService_WithSelfTypeAndKey_ReturnsSelf()
    {
        // Arrange
        var generator = new HuggingFaceEmbeddingGenerator("test-model");

        // Act
        var service = generator.GetService(typeof(HuggingFaceEmbeddingGenerator), generator);

        // Assert
        Assert.Same(generator, service);
    }

    [Fact]
    public void GetService_WithNullServiceKey_ReturnsNull()
    {
        // Arrange
        var generator = new HuggingFaceEmbeddingGenerator("test-model");

        // Act
        var service = generator.GetService(typeof(object), null);

        // Assert
        Assert.Null(service);
    }

    [Fact]
    public void GetService_WithUnknownType_ReturnsNull()
    {
        // Arrange
        var generator = new HuggingFaceEmbeddingGenerator("test-model");

        // Act
        var service = generator.GetService(typeof(string));

        // Assert
        Assert.Null(service);
    }

    [Fact]
    public void GetService_WithNullServiceType_ThrowsArgumentNullException()
    {
        // Arrange
        var generator = new HuggingFaceEmbeddingGenerator("test-model");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => generator.GetService(null!));
    }

    private static bool GetIsExternalHttpClient(HuggingFaceEmbeddingGenerator generator)
    {
        var field = typeof(HuggingFaceEmbeddingGenerator)
            .GetField("_isExternalHttpClient", BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (bool)field.GetValue(generator)!;
    }
}

// Extension method to track HttpClient dispose calls
public static class HttpClientExtensions
{
    private static readonly FieldInfo s_disposedField = typeof(HttpClient)
        .GetField("_disposed", BindingFlags.NonPublic | BindingFlags.Instance)!;

    public static bool DisposeHasBeenCalled(this HttpClient client)
    {
        return (bool)s_disposedField.GetValue(client)!;
    }
}
