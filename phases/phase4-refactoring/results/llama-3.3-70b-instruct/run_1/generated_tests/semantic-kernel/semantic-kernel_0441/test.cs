using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ChromaClientTests
{
    public class ChromaClientTests
    {
        [Fact]
        public async Task ListCollectionsAsync_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent("[{\"Name\":\"collection1\"}]"),
               });
            var httpClient = new HttpClient(handlerMock.Object);
            var chromaClient = new ChromaClient(httpClient, "https://example.com", loggerFactoryMock.Object);

            // Act
            await foreach (var _ in chromaClient.ListCollectionsAsync())
            {
            }

            // Assert
            loggerMock.Verify(l => l.LogDebug("Listing collections"), Times.Once);
        }

        [Fact]
        public async Task GetEmbeddingsAsync_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent("{\"Embeddings\":[]}")
               });
            var httpClient = new HttpClient(handlerMock.Object);
            var chromaClient = new ChromaClient(httpClient, "https://example.com", loggerFactoryMock.Object);

            // Act
            await chromaClient.GetEmbeddingsAsync("collectionId", new[] { "id1" });

            // Assert
            loggerMock.Verify(l => l.LogDebug("Getting embeddings from collection with id: {0}", "collectionId"), Times.Once);
        }
    }
}
