using System;
using System.Net.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Xunit;

public class HuggingFaceEmbeddingGeneratorTests
{
    private class TestHttpClient : HttpClient
    {
        public bool DisposeCalled { get; private set; }

        public TestHttpClient() : base()
        {
            this.DisposeCalled = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeCalled = true;
            }
            base.Dispose(disposing);
        }
    }

    [Fact]
    public void Dispose_InternalHttpClient_DisposesHttpClient()
    {
        // Arrange
        var testHttpClient = new TestHttpClient();
        var generator = new HuggingFaceEmbeddingGenerator(
            new Uri("http://localhost"),
            httpClient: null,
            loggerFactory: NullLoggerFactory.Instance);

        // Replace the internal _httpClient with our testHttpClient using reflection
        var httpClientField = typeof(HuggingFaceEmbeddingGenerator).GetField("_httpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        httpClientField.SetValue(generator, testHttpClient);

        // Also set _isExternalHttpClient to false to simulate internal client
        var isExternalField = typeof(HuggingFaceEmbeddingGenerator).GetField("_isExternalHttpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        isExternalField.SetValue(generator, false);

        // Act
        generator.Dispose();

        // Assert
        Assert.True(testHttpClient.DisposeCalled);
    }

    [Fact]
    public void Dispose_ExternalHttpClient_DoesNotDisposeHttpClient()
    {
        // Arrange
        var testHttpClient = new TestHttpClient();
        var generator = new HuggingFaceEmbeddingGenerator(
            new Uri("http://localhost"),
            httpClient: testHttpClient,
            loggerFactory: NullLoggerFactory.Instance);

        // Act
        generator.Dispose();

        // Assert
        Assert.False(testHttpClient.DisposeCalled);
    }
}
