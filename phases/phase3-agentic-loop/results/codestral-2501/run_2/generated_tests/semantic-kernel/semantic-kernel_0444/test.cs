using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Moq;
using Xunit;

public class ChromaClientTests
{
    [Fact]
    public async Task CreateCollectionAsync_CallsExecuteHttpRequestAsync()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        var mockLogger = new Mock<ILogger<ChromaClient>>();
        var chromaClient = new ChromaClient(mockHttpClient.Object, "http://test", Mock.Of<ILoggerFactory>());

        mockHttpClient.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

        // Act
        await chromaClient.CreateCollectionAsync("testCollection");

        // Assert
        mockHttpClient.Verify(
            client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCollectionAsync_CallsExecuteHttpRequestAsync()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        var mockLogger = new Mock<ILogger<ChromaClient>>();
        var chromaClient = new ChromaClient(mockHttpClient.Object, "http://test", Mock.Of<ILoggerFactory>());

        mockHttpClient.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

        // Act
        await chromaClient.GetCollectionAsync("testCollection");

        // Assert
        mockHttpClient.Verify(
            client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteCollectionAsync_CallsExecuteHttpRequestAsync()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        var mockLogger = new Mock<ILogger<ChromaClient>>();
        var chromaClient = new ChromaClient(mockHttpClient.Object, "http://test", Mock.Of<ILoggerFactory>());

        mockHttpClient.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

        // Act
        await chromaClient.DeleteCollectionAsync("testCollection");

        // Assert
        mockHttpClient.Verify(
            client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpsertEmbeddingsAsync_CallsExecuteHttpRequestAsync()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        var mockLogger = new Mock<ILogger<ChromaClient>>();
        var chromaClient = new ChromaClient(mockHttpClient.Object, "http://test", Mock.Of<ILoggerFactory>());

        mockHttpClient.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

        // Act
        await chromaClient.UpsertEmbeddingsAsync("testCollection", new[] { "id1" }, new[] { new ReadOnlyMemory<float>(new float[] { 1.0f }) });

        // Assert
        mockHttpClient.Verify(
            client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetEmbeddingsAsync_CallsExecuteHttpRequestAsync()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        var mockLogger = new Mock<ILogger<ChromaClient>>();
        var chromaClient = new ChromaClient(mockHttpClient.Object, "http://test", Mock.Of<ILoggerFactory>());

        mockHttpClient.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

        // Act
        await chromaClient.GetEmbeddingsAsync("testCollection", new[] { "id1" });

        // Assert
        mockHttpClient.Verify(
            client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteEmbeddingsAsync_CallsExecuteHttpRequestAsync()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        var mockLogger = new Mock<ILogger<ChromaClient>>();
        var chromaClient = new ChromaClient(mockHttpClient.Object, "http://test", Mock.Of<ILoggerFactory>());

        mockHttpClient.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

        // Act
        await chromaClient.DeleteEmbeddingsAsync("testCollection", new[] { "id1" });

        // Assert
        mockHttpClient.Verify(
            client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task QueryEmbeddingsAsync_CallsExecuteHttpRequestAsync()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        var mockLogger = new Mock<ILogger<ChromaClient>>();
        var chromaClient = new ChromaClient(mockHttpClient.Object, "http://test", Mock.Of<ILoggerFactory>());

        mockHttpClient.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

        // Act
        await chromaClient.QueryEmbeddingsAsync("testCollection", new[] { new ReadOnlyMemory<float>(new float[] { 1.0f }) }, 1);

        // Assert
        mockHttpClient.Verify(
            client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteHttpRequestAsync_LogsErrorOnException()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        var mockLogger = new Mock<ILogger<ChromaClient>>();
        var chromaClient = new ChromaClient(mockHttpClient.Object, "http://test", Mock.Of<ILoggerFactory>());

        var request = new HttpRequestMessage(HttpMethod.Get, "test");
        mockHttpClient.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Test exception"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpOperationException>(() => chromaClient.ExecuteHttpRequestAsync(request));

        mockLogger.Verify(
            logger => logger.LogError(
                It.IsAny<EventId>(),
                It.IsAny<Exception>(),
                It.IsAny<string>(),
                It.IsAny<object[]>()),
            Times.Once);
    }
}
