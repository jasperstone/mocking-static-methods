using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ChromaClientTests
{
    public class ChromaClientLoggingTests
    {
        [Fact]
        public async Task ExecuteHttpRequestAsync_Should_LogError_On_HttpOperationException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var httpClientMock = new Mock<HttpClient>();
            var client = new TestChromaClient(httpClientMock.Object, loggerMock.Object);

            var request = new HttpRequestMessage(HttpMethod.Get, "test");
            var cancellationToken = CancellationToken.None;

            // Setup to throw HttpOperationException
            var exception = new HttpOperationException("Error");
            client.SetupSendAsync(request, exception);

            // Act & Assert
            await Assert.ThrowsAsync<HttpOperationException>(() => client.ExecuteHttpRequestAsync(request, cancellationToken));

            // Verify LogError was called
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("operation failed")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        // A derived class to inject mock behavior
        private class TestChromaClient : ChromaClient
        {
            private readonly HttpClient _httpClient;
            private readonly ILogger _logger;
            private HttpRequestMessage _lastRequest;
            private Exception _exceptionToThrow;

            public TestChromaClient(HttpClient httpClient, ILogger logger)
            {
                _httpClient = httpClient;
                _logger = logger;
            }

            public void SetupSendAsync(HttpRequestMessage request, Exception exception)
            {
                _lastRequest = request;
                _exceptionToThrow = exception;
            }

            protected override async Task<(HttpResponseMessage, string)> ExecuteHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                if (request == _lastRequest && _exceptionToThrow != null)
                {
                    throw (HttpOperationException)_exceptionToThrow;
                }
                return await base.ExecuteHttpRequestAsync(request, cancellationToken);
            }
        }
    }
}
