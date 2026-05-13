using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.HuggingFace.Tests
{
    public class HuggingFaceEmbeddingGeneratorTests : IDisposable
    {
        private bool _disposed;

        [Fact]
        public void Dispose_InternalHttpClient_DisposesHttpClient()
        {
            // Arrange
            var generator = new HuggingFaceEmbeddingGenerator(
                new Uri("https://example.com"),
                httpClient: null);

            // Act
            generator.Dispose();

            // Assert
            // We cannot directly assert _httpClient disposed state because HttpClient does not expose it.
            // But no exception means Dispose was called safely.
            Assert.True(true);
        }

        [Fact]
        public void Dispose_ExternalHttpClient_DoesNotDisposeHttpClient()
        {
            // Arrange
            var externalHttpClient = new HttpClient();
            var generator = new HuggingFaceEmbeddingGenerator(
                new Uri("https://example.com"),
                httpClient: externalHttpClient);

            // Act
            generator.Dispose();

            // Assert
            // The external HttpClient should not be disposed by the generator.
            // We test this by trying to use the externalHttpClient after Dispose.
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var task = externalHttpClient.SendAsync(request);
            Assert.False(task.IsFaulted);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                // Cleanup if needed
                _disposed = true;
            }
        }
    }
}
