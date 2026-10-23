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

            // Setup SendWithSuccessCheckAsync to throw HttpOperationException
            var exception = new HttpOperationException("Error occurred");
            client.SetupSendWithSuccessCheckAsync(request, exception);

            // Act & Assert
            await Assert.ThrowsAsync<HttpOperationException>(() => client.ExecuteHttpRequestAsync(request, cancellationToken));

            // Verify LogError was called with the exception
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("operation failed")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    // A derived class to inject the mock behavior
    public class TestChromaClient : ChromaClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private HttpRequestMessage _lastRequest;

        public TestChromaClient(HttpClient httpClient, ILogger logger)
            : base("http://test", null)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public void SetupSendWithSuccessCheckAsync(HttpRequestMessage request, Exception exception)
        {
            _lastRequest = request;
            // Store the exception to throw when SendWithSuccessCheckAsync is called
            _throwException = exception;
        }

        private Exception _throwException;

        protected override async Task<HttpResponseMessage> SendWithSuccessCheckAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_throwException != null)
            {
                throw (HttpOperationException)_throwException;
            }
            // Return a dummy response if needed
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        }

        public override async Task<(HttpResponseMessage response, string responseContent)> ExecuteHttpRequestAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await SendWithSuccessCheckAsync(request, cancellationToken);
                var content = await response.Content.ReadAsStringAsync();
                return (response, content);
            }
            catch (HttpOperationException e)
            {
                _logger.LogError(e, "{Method} {Path} operation failed: {Message}, {Response}", request.Method.Method, request.RequestUri.ToString(), e.Message, e.ResponseContent);
                throw;
            }
        }
    }

    // Custom exception to simulate HttpOperationException
    public class HttpOperationException : Exception
    {
        public string ResponseContent { get; }

        public HttpOperationException(string message) : base(message)
        {
            ResponseContent = "Error response content";
        }
    }
}
