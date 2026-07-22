using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Chroma;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json;
using System.Collections.Generic;
using System.Net;

namespace ChromaClientTests
{
    public class ChromaClientTests
    {
        [Fact]
        public async Task GetEmbeddingsAsync_LogsDebugMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ChromaClient>>();
            var mockHttpClient = new Mock<HttpClient>();
            var chromaClient = new ChromaClient(mockHttpClient.Object, "http://testendpoint", Mock.Of<ILoggerFactory>());

            var collectionId = "testCollectionId";
            var ids = new[] { "id1", "id2" };
            var include = new[] { "include1", "include2" };

            var responseContent = JsonSerializer.Serialize(new ChromaEmbeddingsModel());
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent)
            };

            mockHttpClient.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMessage);

            // Act
            await chromaClient.GetEmbeddingsAsync(collectionId, ids, include);

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
