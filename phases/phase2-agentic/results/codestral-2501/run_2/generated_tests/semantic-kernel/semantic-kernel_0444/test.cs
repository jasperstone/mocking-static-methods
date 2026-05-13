using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
            var chromaClient = new ChromaClient(httpClientMock.Object, "http://testendpoint", Mock.Of<ILoggerFactory>());

            var request = new HttpRequestMessage(HttpMethod.Get, "http://testendpoint/api/v1/test");
            var cancellationToken = new CancellationToken();

            httpClientMock.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpOperationException("Test error", new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)));

            // Act
            await Assert.ThrowsAsync<HttpOperationException>(() => chromaClient.ExecuteHttpRequestAsync(request, cancellationToken));

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }
    }
}
