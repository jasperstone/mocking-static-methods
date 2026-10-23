using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ChromaClientTests
{
    public class ExecuteHttpRequestAsyncTests
    {
        [Fact]
        public async Task ExecuteHttpRequestAsync_Should_LogError_When_HttpOperationExceptionThrown()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockHttpClient = new Mock<HttpClient>();
            var client = new TestChromaClient(mockHttpClient.Object, mockLogger.Object);

            var request = new HttpRequestMessage(HttpMethod.Get, "test");
            var cts = new CancellationToken();

            // Simulate SendWithSuccessCheckAsync throwing HttpOperationException
            var exception = new HttpOperationException("Error response");
            mockHttpClient
                .Setup(c => c.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            // Act
            await Assert.ThrowsAsync<HttpOperationException>(() => client.InvokeExecuteHttpRequestAsync(request, cts));

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("operation failed")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    // Helper class to access the protected method
    public class TestChromaClient : ChromaClient
    {
        public TestChromaClient(HttpClient httpClient, ILogger logger)
            : base(httpClient, null)
        {
            _logger = logger;
        }

        public Task<(HttpResponseMessage, string)> InvokeExecuteHttpRequestAsync(HttpRequestMessage request, CancellationToken token)
        {
            return base.ExecuteHttpRequestAsync(request, token);
        }
    }

    // Mock of HttpOperationException
    public class HttpOperationException : Exception
    {
        public string ResponseContent { get; }

        public HttpOperationException(string message) : base(message)
        {
            ResponseContent = "Error response";
        }
    }
}
