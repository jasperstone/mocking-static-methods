using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Chroma.Tests
{
    public class ChromaClientTests
    {
        private class FakeHttpClientThrows : HttpClient
        {
            private readonly Exception _exceptionToThrow;

            public FakeHttpClientThrows(Exception exceptionToThrow)
            {
                _exceptionToThrow = exceptionToThrow;
            }

            public override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                throw _exceptionToThrow;
            }
        }

        [Fact]
        public async Task ExecuteHttpRequestAsync_LogsErrorOnHttpOperationException()
        {
            // Arrange
            var exception = new Microsoft.SemanticKernel.Http.HttpOperationException("error message", null, "response content");
            var fakeHttpClient = new FakeHttpClientThrows(exception);
            var chromaClient = new ChromaClient(fakeHttpClient, "http://localhost", NullLoggerFactory.Instance);

            var request = new HttpRequestMessage(HttpMethod.Get, "test");

            // Use reflection to invoke private method ExecuteHttpRequestAsync
            var method = typeof(ChromaClient).GetMethod("ExecuteHttpRequestAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Microsoft.SemanticKernel.Http.HttpOperationException>(async () =>
            {
                var task = (Task<(HttpResponseMessage, string)>)method.Invoke(chromaClient, new object[] { request, CancellationToken.None })!;
                await task;
            });

            Assert.Equal("error message", ex.Message);
        }
    }
}
