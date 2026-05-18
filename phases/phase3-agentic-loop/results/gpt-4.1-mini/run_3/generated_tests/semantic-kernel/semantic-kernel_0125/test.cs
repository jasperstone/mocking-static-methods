using System;
using System.Net.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.HuggingFace.Tests
{
    public class HuggingFaceEmbeddingGeneratorTests
    {
        [Fact]
        public void Dispose_InternalHttpClient_DisposesHttpClient()
        {
            // Arrange
            var disposed = false;
            var handler = new DisposeTrackingHandler(() => disposed = true);
            var httpClient = new HttpClient(handler);
            var endpoint = new Uri("http://localhost");

            // Act
            var generator = new HuggingFaceEmbeddingGenerator(endpoint, httpClient: null, loggerFactory: NullLoggerFactory.Instance);
            generator.Dispose();

            // Assert
            Assert.True(disposed);
        }

        [Fact]
        public void Dispose_ExternalHttpClient_DoesNotDisposeHttpClient()
        {
            // Arrange
            var disposed = false;
            var handler = new DisposeTrackingHandler(() => disposed = true);
            var httpClient = new HttpClient(handler);
            var endpoint = new Uri("http://localhost");

            // Act
            var generator = new HuggingFaceEmbeddingGenerator(endpoint, httpClient: httpClient, loggerFactory: NullLoggerFactory.Instance);
            generator.Dispose();

            // Assert
            Assert.False(disposed);
        }

        private class DisposeTrackingHandler : HttpMessageHandler
        {
            private readonly Action _onDispose;

            public DisposeTrackingHandler(Action onDispose)
            {
                _onDispose = onDispose;
            }

            protected override System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                if (disposing)
                {
                    _onDispose();
                }
            }
        }
    }
}
