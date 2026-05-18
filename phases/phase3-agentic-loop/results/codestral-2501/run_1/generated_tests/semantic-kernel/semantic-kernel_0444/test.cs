using Xunit;
using Moq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Microsoft.SemanticKernel.Http;
using System;
using System.Reflection;

namespace ChromaClientTests
{
    public class ChromaClientTests
    {
        [Fact]
        public async Task ExecuteHttpRequestAsync_ShouldLogError_WhenHttpOperationExceptionIsThrown()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ChromaClient>>();
            var mockHttpClient = new Mock<HttpClient>();
            var chromaClient = new ChromaClient(mockHttpClient.Object, "http://testendpoint", Mock.Of<ILoggerFactory>());

            var request = new HttpRequestMessage(HttpMethod.Get, "http://testendpoint/api/v1/test");
            var exception = new HttpOperationException("Test exception", new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest));

            mockHttpClient.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            // Act
            var method = typeof(ChromaClient).GetMethod("ExecuteHttpRequestAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            await Assert.ThrowsAsync<HttpOperationException>(() => (Task)method.Invoke(chromaClient, new object[] { request, CancellationToken.None }));

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(
                    It.IsAny<EventId>(),
                    exception,
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
