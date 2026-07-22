using System;
using System.Net.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Xunit;

public class HuggingFaceEmbeddingGeneratorTests
{
    private class HttpClientWithDisposeTracking : HttpClient
    {
        public bool DisposeCalled { get; private set; } = false;

        protected override void Dispose(bool disposing)
        {
            DisposeCalled = true;
            base.Dispose(disposing);
        }
    }

    [Fact]
    public void Dispose_InternalHttpClient_DisposeCalled()
    {
        // Arrange
        var generator = new HuggingFaceEmbeddingGenerator(
            modelId: "test-model",
            endpoint: new Uri("http://localhost"),
            httpClient: null,
            loggerFactory: NullLoggerFactory.Instance);

        // Use reflection to get the private _httpClient field and replace it with our tracking HttpClient
        var httpClientField = typeof(HuggingFaceEmbeddingGenerator).GetField("_httpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var trackingClient = new HttpClientWithDisposeTracking();
        httpClientField.SetValue(generator, trackingClient);

        // Also set _isExternalHttpClient to false to simulate internal client
        var isExternalField = typeof(HuggingFaceEmbeddingGenerator).GetField("_isExternalHttpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        isExternalField.SetValue(generator, false);

        // Act
        generator.Dispose();

        // Assert
        Assert.True(trackingClient.DisposeCalled);
    }

    [Fact]
    public void Dispose_ExternalHttpClient_DisposeNotCalled()
    {
        // Arrange
        var trackingClient = new HttpClientWithDisposeTracking();
        var generator = new HuggingFaceEmbeddingGenerator(
            modelId: "test-model",
            endpoint: new Uri("http://localhost"),
            httpClient: trackingClient,
            loggerFactory: NullLoggerFactory.Instance);

        // Act
        generator.Dispose();

        // Assert
        Assert.False(trackingClient.DisposeCalled);
    }
}
