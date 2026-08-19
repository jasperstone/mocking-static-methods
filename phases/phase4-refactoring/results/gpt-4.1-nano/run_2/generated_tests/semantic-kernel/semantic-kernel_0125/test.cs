using System;
using System.Net.Http;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.HuggingFace.Tests
{
    public class HuggingFaceEmbeddingGeneratorTests
    {
        private class TestHttpClient : HttpClient
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
            var httpClient = new TestHttpClient();
            var generator = new HuggingFaceEmbeddingGenerator(
                modelId: "test-model",
                endpoint: new Uri("https://test.endpoint"),
                httpClient: httpClient
            );

            // Act
            generator.Dispose();

            // Assert
            Assert.True(httpClient.IsDisposed);
        }
    }
}
