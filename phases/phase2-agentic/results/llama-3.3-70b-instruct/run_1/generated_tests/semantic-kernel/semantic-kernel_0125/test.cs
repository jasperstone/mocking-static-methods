using Xunit;
using System;
using System.Net.Http;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public class HuggingFaceEmbeddingGeneratorTests
{
    [Fact]
    public void Dispose_DisposesHttpClient_WhenCreatedInternally()
    {
        // Arrange
        var httpClient = new HttpClient();
        var loggerFactory = new LoggerFactory();
        var embeddingGenerator = new HuggingFaceEmbeddingGenerator(
            endpoint: new Uri("https://example.com"),
            apiKey: null,
            httpClient: null,
            loggerFactory: loggerFactory);

        // Act
        embeddingGenerator.Dispose();

        // Assert
        Assert.Throws<ObjectDisposedException>(() => embeddingGenerator._httpClient.Dispose());
    }

    [Fact]
    public void Dispose_DoesNotDisposeHttpClient_WhenCreatedExternally()
    {
        // Arrange
        var httpClient = new HttpClient();
        var loggerFactory = new LoggerFactory();
        var embeddingGenerator = new HuggingFaceEmbeddingGenerator(
            endpoint: new Uri("https://example.com"),
            apiKey: null,
            httpClient: httpClient,
            loggerFactory: loggerFactory);

        // Act
        embeddingGenerator.Dispose();

        // Assert
        Assert.DoesNotThrow(() => embeddingGenerator._httpClient.Dispose());
    }
}
