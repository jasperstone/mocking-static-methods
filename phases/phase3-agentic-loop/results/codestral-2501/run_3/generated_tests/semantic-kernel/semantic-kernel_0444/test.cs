using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Moq;
using Xunit;

namespace ChromaClientTests
{
    public class ChromaClientTests
    {
        [Fact]
        public async Task ExecuteHttpRequestAsync_ShouldLogError_WhenHttpOperationExceptionIsThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ChromaClient>>();
            var httpClientMock = new Mock<HttpClient>();
            var chromaClient = new ChromaClient(httpClientMock.Object, "http://testendpoint", new LoggerFactory());

            var request = new HttpRequestMessage(HttpMethod.Get, "http://testendpoint/api/v1/test");
            var exception = new HttpOperationException("Test exception", new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError));

            httpClientMock.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            // Act
            await Assert.ThrowsAsync<HttpOperationException>(() => chromaClient.ExecuteHttpRequestAsync(request));

            // Assert
            loggerMock.Verify(
                logger => logger.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<HttpOperationException>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
