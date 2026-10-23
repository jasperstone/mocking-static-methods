using System;
using System.Net.Http;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.HuggingFace.Services;

namespace SemanticKernel.Tests
{
    public class HuggingFaceEmbeddingGeneratorTests
    {
        private class DisposableHttpClient : HttpClient
        {
            public bool IsDisposed { get; private set; } = false;

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                IsDisposed = true;
            }
        }

        [Fact]
        public void Dispose_Should_Dispose_HttpClient_When_Internal()
        {
            // Arrange
            var disposableHttpClient = new DisposableHttpClient();
            var generator = new HuggingFaceEmbeddingGenerator(
                modelId: "test-model",
                endpoint: new Uri("https://fakeendpoint"),
                httpClient: disposableHttpClient,
                loggerFactory: null
            );

            // Act
            generator.Dispose();

            // Assert
            Assert.True(disposableHttpClient.IsDisposed);
        }
    }
}
