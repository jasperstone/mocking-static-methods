using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Microsoft.SemanticKernel.Http;
using Moq;
using Xunit;

namespace ChromaClientTests
{
    public class ChromaClientTests
    {
        [Fact]
        public async Task ExecuteHttpRequestAsync_ShouldLogError_WhenExceptionThrown()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ChromaClient>>();
            var mockHttpClient = new Mock<HttpClient>();
            var chromaClient = new ChromaClient(mockHttpClient.Object, "http://testendpoint", new LoggerFactory());

            var request = new HttpRequestMessage(HttpMethod.Get, "http://testendpoint/api/v1/test");
            var exception = new HttpOperationException("Test exception", new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest));

            mockHttpClient.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            // Act
            await Assert.ThrowsAsync<HttpOperationException>(() => chromaClient.ExecuteHttpRequestAsync(request));

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(
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
