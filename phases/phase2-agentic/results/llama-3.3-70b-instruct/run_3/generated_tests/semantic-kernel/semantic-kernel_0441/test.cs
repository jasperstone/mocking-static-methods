using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Chroma;

namespace ChromaClientTests
{
    public class ChromaClientTests
    {
        [Fact]
        public async Task ListCollectionsAsync_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ChromaClient>>();
            var chromaClient = new ChromaClient("https://example.com", loggerMock.Object);
            var httpClientMock = new Mock<HttpClient>();
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api/v1/collections");
            var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("[{\"Name\":\"Collection1\"},{\"Name\":\"Collection2\"}]")
            };

            httpClientMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponseMessage);

            chromaClient._httpClient = httpClientMock.Object;

            // Act
            await chromaClient.ListCollectionsAsync();

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => string.Equals("Listing collections", o.ToString(), StringComparison.OrdinalIgnoreCase)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task GetEmbeddingsAsync_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ChromaClient>>();
            var chromaClient = new ChromaClient("https://example.com", loggerMock.Object);
            var httpClientMock = new Mock<HttpClient>();
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api/v1/collections/Collection1/embeddings");
            var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("{\"Embeddings\":[{\"Id\":\"Embedding1\",\"Vector\":[1.0,2.0,3.0]}]}")
            };

            httpClientMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponseMessage);

            chromaClient._httpClient = httpClientMock.Object;

            // Act
            await chromaClient.GetEmbeddingsAsync("Collection1", new[] { "Embedding1" });

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => string.Equals("Getting embeddings from collection with id: Collection1", o.ToString(), StringComparison.OrdinalIgnoreCase)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.Once
            );
        }
    }
}
