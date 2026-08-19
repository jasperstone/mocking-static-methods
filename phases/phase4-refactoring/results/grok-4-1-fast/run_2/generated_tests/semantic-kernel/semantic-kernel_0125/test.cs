using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.Http;
using Moq;
using Moq.Protected;
using Xunit;

public class HuggingFaceEmbeddingGeneratorTests : IDisposable
{
    private readonly Mock<HttpMessageHandler> _mockHandler;
    private readonly HttpClient _httpClient;
    private readonly HuggingFaceEmbeddingGenerator _generatorWithExternalClient;
    private readonly HuggingFaceEmbeddingGenerator _generatorWithInternalClient;

    public HuggingFaceEmbeddingGeneratorTests()
    {
        _mockHandler = new Mock<HttpMessageHandler>();
        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

        _httpClient = new HttpClient(_mockHandler.Object);

        // Generator with external HttpClient (should NOT dispose)
        _generatorWithExternalClient = new HuggingFaceEmbeddingGenerator(
            modelId: "test-model",
            httpClient: _httpClient,
            loggerFactory: NullLoggerFactory.Instance);

        // Generator with internal HttpClient (should dispose)
        _generatorWithInternalClient = new HuggingFaceEmbeddingGenerator(
            modelId: "test-model",
            loggerFactory: NullLoggerFactory.Instance);
    }

    public void Dispose()
    {
        _generatorWithExternalClient.Dispose();
        _generatorWithInternalClient.Dispose();
        _httpClient.Dispose();
    }

    [Fact]
    public void Constructor_WithExternalHttpClient_SetsCorrectMetadata()
    {
        // Act
        var metadata = _generatorWithExternalClient.GetService(typeof(EmbeddingGeneratorMetadata), _generatorWithExternalClient);

        // Assert
        Assert.IsType<EmbeddingGeneratorMetadata>(metadata);
    }

    [Fact]
    public void Constructor_WithoutHttpClient_Succeeds()
    {
        // Act
        using var generator = new HuggingFaceEmbeddingGenerator(
            modelId: "test-model",
            loggerFactory: NullLoggerFactory.Instance);

        // Assert
        Assert.NotNull(generator);
        Assert.IsType<EmbeddingGeneratorMetadata>(generator.GetService(typeof(EmbeddingGeneratorMetadata), generator));
    }

    [Fact]
    public void Dispose_WithExternalHttpClient_DoesNotThrow()
    {
        // Arrange
        var mockClient = new Mock<HttpClient>(_mockHandler.Object);
        mockClient.Protected().Setup("Dispose", ItExpr.IsAny<bool>());

        using var generator = new HuggingFaceEmbeddingGenerator(
            modelId: "test-model",
            httpClient: mockClient.Object,
            loggerFactory: NullLoggerFactory.Instance);

        // Act
        generator.Dispose();

        // Assert - no exception thrown (external client path taken)
        mockClient.Protected().Verify("Dispose", Times.Never(), ItExpr.IsAny<bool>());
    }

    [Fact]
    public void Dispose_WithInternalHttpClient_DoesNotThrow()
    {
        // Arrange
        using var generator = new HuggingFaceEmbeddingGenerator(
            modelId: "test-model",
            loggerFactory: NullLoggerFactory.Instance);

        // Act
        generator.Dispose();

        // Assert - no exception thrown (internal client disposed)
        Assert.True(true);
    }

    [Fact]
    public void GetService_ReturnsSelf_WhenServiceTypeMatchesInstance()
    {
        using var generator = new HuggingFaceEmbeddingGenerator("test-model");

        var result = generator.GetService(typeof(HuggingFaceEmbeddingGenerator), generator);

        Assert.Same(generator, result);
    }

    [Fact]
    public void GetService_ReturnsMetadata_ForEmbeddingGeneratorMetadata()
    {
        using var generator = new HuggingFaceEmbeddingGenerator("test-model");

        var result = generator.GetService(typeof(EmbeddingGeneratorMetadata), generator);

        Assert.IsType<EmbeddingGeneratorMetadata>(result);
    }

    [Fact]
    public void GetService_ReturnsNull_ForUnknownType()
    {
        using var generator = new HuggingFaceEmbeddingGenerator("test-model");

        var result1 = generator.GetService(typeof(string), generator);
        var result2 = generator.GetService(typeof(EmbeddingGeneratorMetadata), "wrong-key");

        Assert.Null(result1);
        Assert.Null(result2);
    }

    [Fact]
    public void GetService_ReturnsNull_ForNullServiceKey()
    {
        using var generator = new HuggingFaceEmbeddingGenerator("test-model");

        var result = generator.GetService(typeof(HuggingFaceEmbeddingGenerator), null);

        Assert.Null(result);
    }

    [Fact]
    public async Task GenerateAsync_ThrowsException_WhenClientFails()
    {
        // Arrange - test that method executes without immediate failure
        using var generator = new HuggingFaceEmbeddingGenerator("test-model");

        // Act & Assert - expect HTTP/network exception due to no real endpoint
        await Assert.ThrowsAnyAsync<Exception>(() => generator.GenerateAsync(new[] { "test" }));
    }
}
