using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Http;
using Microsoft.SemanticKernel.Connectors.Chroma;

namespace ChromaClientTests
{
    public class ChromaClientLoggingTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<HttpClient> _httpClientMock;

        public ChromaClientLoggingTests()
        {
            _loggerMock = new Mock<ILogger>();
            _httpClientMock = new Mock<HttpClient>();
        }

        [Fact]
        public async Task ExecuteHttpRequestAsync_Should_LogError_When_HttpOperationExceptionThrown()
        {
            // Arrange
            var client = new ChromaClient("http://test", null);
            var request = new HttpRequestMessage(HttpMethod.Get, "test");
            var exception = new HttpOperationException("Error response", "Response content");
            var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);

            // Setup the HttpClient to throw
            var clientMock = new Mock<IHttpClient>();
            clientMock.Setup(c => c.SendWithSuccessCheckAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            // Inject the mock HttpClient into the ChromaClient
            var chromaClient = new ChromaClient("http://test", null);
            // Replace the private _httpClient with our mock
            typeof(ChromaClient).GetField("_httpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(chromaClient, clientMock.Object);
            // Replace the private _logger with our mock
            typeof(ChromaClient).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(chromaClient, _loggerMock.Object);

            // Act
            await Assert.ThrowsAsync<HttpOperationException>(async () =>
            {
                await chromaClient.ExecuteHttpRequestAsync(request);
            });

            // Assert
            _loggerMock.Verify(
                x => x.LogError(
                    exception,
                    "{Method} {Path} operation failed: {Message}, {Response}",
                    request.Method.Method,
                    request.RequestUri.ToString(),
                    exception.Message,
                    exception.ResponseContent),
                Times.Once);
        }
    }
}
