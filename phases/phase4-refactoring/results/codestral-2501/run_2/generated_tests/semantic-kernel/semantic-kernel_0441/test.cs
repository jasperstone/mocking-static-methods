using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.SemanticKernel.Connectors.Chroma;
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
            var mockHttpResponseMessage = new Mock<HttpResponseMessage>();
            var mockHttpContent = new Mock<HttpContent>();

            var collectionId = "testCollection";
            var ids = new[] { "id1", "id2" };
            var include = new[] { "include1", "include2" };

            var expectedResponseContent = JsonSerializer.Serialize(new ChromaEmbeddingsModel());
            mockHttpContent.Setup(c => c.ReadAsStringAsync()).ReturnsAsync(expectedResponseContent);
            mockHttpResponseMessage.Setup(r => r.Content).Returns(mockHttpContent.Object);
            mockHttpResponseMessage.Setup(r => r.StatusCode).Returns(HttpStatusCode.OK);

            mockHttpClient.Setup(c => c.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockHttpResponseMessage.Object);

            var chromaClient = new ChromaClient(mockHttpClient.Object, "http://testendpoint", Mock.Of<ILoggerFactory>());

            // Act
            var result = await chromaClient.GetEmbeddingsAsync(collectionId, ids, include);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Getting embeddings from collection with id: testCollection")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            Assert.NotNull(result);
        }
    }
}
