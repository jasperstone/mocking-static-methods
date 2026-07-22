using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Chroma.Tests
{
    public class ChromaClientTests
    {
        [Fact]
        public async Task ExecuteHttpRequestAsync_LogsErrorOnHttpOperationException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var httpClient = new HttpClient();
            var testClient = new TestChromaClient(httpClient, "http://localhost/", loggerMock.Object);

            var request = new HttpRequestMessage(HttpMethod.Get, "test");

            // Act & Assert
            var ex = await Assert.ThrowsAsync<HttpOperationException>(() => testClient.ExecuteHttpRequestAsync(request));

            // Verify that LogError was called with expected parameters
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("operation failed")),
                    ex,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class TestChromaClient : ChromaClient
        {
            private readonly ILogger _logger;

            public TestChromaClient(HttpClient httpClient, string? endpoint, ILogger logger)
                : base(httpClient, endpoint, NullLoggerFactory.Instance)
            {
                _logger = logger;
            }

            public new async Task<(HttpResponseMessage response, string responseContent)> ExecuteHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
            {
                try
                {
                    throw new HttpOperationException("Test exception", "Test response content");
                }
                catch (HttpOperationException e)
                {
                    _logger.LogError(e, "{Method} {Path} operation failed: {Message}, {Response}", request.Method.Method, request.RequestUri?.ToString(), e.Message, e.ResponseContent);
                    throw;
                }
            }
        }

        // Minimal HttpOperationException class for testing
        private class HttpOperationException : Exception
        {
            public string ResponseContent { get; }

            public HttpOperationException(string message, string responseContent) : base(message)
            {
                ResponseContent = responseContent;
            }
        }
    }
}
