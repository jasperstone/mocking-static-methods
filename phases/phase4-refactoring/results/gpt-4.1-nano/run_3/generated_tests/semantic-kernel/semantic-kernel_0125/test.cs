using System;
using System.Net.Http;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.HuggingFace; // Assuming this namespace contains the class

namespace HuggingFaceEmbeddingGeneratorTests
{
    // Fake HttpMessageHandler to track disposal
    public class TrackingHttpMessageHandler : HttpMessageHandler
    {
        public bool IsDisposed { get; private set; } = false;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            IsDisposed = true;
        }

        protected override System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            // Return a dummy response
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            return System.Threading.Tasks.Task.FromResult(response);
        }
    }

    public class HuggingFaceEmbeddingGeneratorTests
    {
        [Fact]
        public void Dispose_Should_Dispose_HttpClientHandler_When_Internal()
        {
            // Arrange
            var handler = new TrackingHttpMessageHandler();
            var httpClient = new HttpClient(handler);
            var generator = new HuggingFaceEmbeddingGenerator(
                modelId: "test-model",
                endpoint: new Uri("https://fakeendpoint"),
                httpClient: httpClient,
                loggerFactory: NullLoggerFactory.Instance
            );

            // Act
            generator.Dispose();

            // Assert
            Assert.True(handler.IsDisposed);
        }
    }
}
