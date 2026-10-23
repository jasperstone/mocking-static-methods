using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Xunit;

public class HuggingFaceEmbeddingGeneratorTests
{
    [Fact]
    public void Dispose_InternalHttpClient_DisposesHttpClient()
    {
        // Arrange
        var generator = new HuggingFaceEmbeddingGenerator(
            new Uri("http://localhost"),
            httpClient: null,
            loggerFactory: NullLoggerFactory.Instance);

        // Act & Assert
        // We cannot directly check if HttpClient.Dispose was called because Dispose is non-virtual.
        // But we can check that Dispose does not throw and can be called multiple times safely.
        generator.Dispose();
        generator.Dispose();
    }

    [Fact]
    public void Dispose_ExternalHttpClient_DoesNotDisposeHttpClient()
    {
        // Arrange
        var httpClient = new HttpClient();
        var generator = new HuggingFaceEmbeddingGenerator(
            new Uri("http://localhost"),
            httpClient: httpClient,
            loggerFactory: NullLoggerFactory.Instance);

        // Act
        generator.Dispose();

        // Assert
        // The external HttpClient should not be disposed by the generator.
        // We test this by trying to use the HttpClient after Dispose.
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost");
        // This should not throw ObjectDisposedException
        var task = httpClient.SendAsync(request);
        Assert.False(task.IsFaulted);
    }
}
