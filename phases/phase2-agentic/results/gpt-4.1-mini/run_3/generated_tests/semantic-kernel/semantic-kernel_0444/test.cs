using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Chroma.Tests
{
    public class ChromaClientTests
    {
        private class TestHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsync;

            public TestHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
            {
                _sendAsync = sendAsync;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return _sendAsync(request, cancellationToken);
            }
        }

        [Fact]
        public async Task ExecuteHttpRequestAsync_LogsErrorOnHttpOperationException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var httpClient = new HttpClient(new TestHttpMessageHandler((req, ct) =>
            {
                // Simulate throwing HttpOperationException
                var ex = new HttpOperationException("Test error", "Response content");
                throw ex;
            }));

            var client = new ChromaClient(httpClient, "http://localhost/", loggerFactory: null);
            // Replace private logger with mock via reflection
            var loggerField = typeof(ChromaClient).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(client, loggerMock.Object);

            var request = new HttpRequestMessage(HttpMethod.Get, "test");

            // Act & Assert
            var ex = await Assert.ThrowsAsync<HttpOperationException>(async () =>
            {
                // Use reflection to call private method ExecuteHttpRequestAsync
                var method = typeof(ChromaClient).GetMethod("ExecuteHttpRequestAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var task = (Task<(HttpResponseMessage, string)>)method.Invoke(client, new object[] { request, CancellationToken.None });
                await task;
            });

            // Verify LogError was called with expected parameters
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("operation failed")),
                    ex,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Custom HttpOperationException class to simulate the exception used in ChromaClient
    public class HttpOperationException : Exception
    {
        public string ResponseContent { get; }

        public HttpOperationException(string message, string responseContent) : base(message)
        {
            this.ResponseContent = responseContent;
        }
    }
}
