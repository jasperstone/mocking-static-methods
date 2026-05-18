using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ChromaClientTests
{
    public class ChromaClientTests
    {
        [Fact]
        public async Task GetEmbeddingsAsync_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ChromaClient>>();
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            var httpClient = new HttpClient(handlerMock.Object);
            var chromaClient = new ChromaClient(httpClient, "https://example.com");

            // Act
            await chromaClient.GetEmbeddingsAsync("collectionId", new[] { "id1", "id2" }, null, default);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [Fact]
        public async Task GetEmbeddingsAsync_ThrowsArgumentNullException_WhenCollectionIdIsNull()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            var httpClient = new HttpClient(handlerMock.Object);
            var chromaClient = new ChromaClient(httpClient, "https://example.com");

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => chromaClient.GetEmbeddingsAsync(null, new[] { "id1", "id2" }, null, default));
        }

        [Fact]
        public async Task GetEmbeddingsAsync_ThrowsArgumentNullException_WhenIdsIsNull()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            var httpClient = new HttpClient(handlerMock.Object);
            var chromaClient = new ChromaClient(httpClient, "https://example.com");

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => chromaClient.GetEmbeddingsAsync("collectionId", null, null, default));
        }
    }
}
