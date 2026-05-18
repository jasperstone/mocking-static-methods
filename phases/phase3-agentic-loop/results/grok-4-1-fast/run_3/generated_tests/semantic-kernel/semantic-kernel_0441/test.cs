using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Microsoft.SemanticKernel.Http;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Tests.Chroma;

public class ChromaClientTests
{
    [Fact]
    public async Task GetEmbeddingsAsync_LogsDebugMessageWithCorrectCollectionId()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

        var mockHttpClient = new Mock<HttpClient>();
        mockHttpClient.Setup(c => c.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                     {
                         Content = new StringContent("{\"embeddings\": []}")
                     });

        var mockHttpClientExtensions = new Mock<HttpClientExtensions>();
        // Mock the extension method using reflection or just let it use the real one since we control the HttpClient

        var client = new ChromaClient(mockHttpClient.Object, null, loggerFactory.Object)
        {
            // Set BaseAddress to avoid endpoint validation issues
            _httpClient = { BaseAddress = new Uri("http://localhost/") }
        };

        // Act
        await client.GetEmbeddingsAsync("test-collection-id", new[] { "id1" });

        // Assert - Verify LogDebug extension was called with correct message and collectionId
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains("Getting embeddings from collection with id: test-collection-id")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
