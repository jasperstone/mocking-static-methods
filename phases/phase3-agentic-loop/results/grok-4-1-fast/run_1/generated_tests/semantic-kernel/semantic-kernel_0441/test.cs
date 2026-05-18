using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Chroma.UnitTests;

public class ChromaClientTests
{
    private readonly Mock<HttpClient> _httpClientMock;
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;
    private readonly Mock<ILogger<ChromaClient>> _loggerMock;
    private readonly ChromaClient _client;

    public ChromaClientTests()
    {
        _httpClientMock = new Mock<HttpClient>();
        _loggerMock = new Mock<ILogger<ChromaClient>>();
        _loggerFactoryMock = new Mock<ILoggerFactory>();
        _loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(_loggerMock.Object);

        _client = new ChromaClient(_httpClientMock.Object, endpoint: "http://localhost:8000", loggerFactory: _loggerFactoryMock.Object);
    }

    [Fact]
    public async Task GetEmbeddingsAsync_LogsDebugMessageWithCollectionId()
    {
        // Arrange
        var collectionId = "test-collection";
        var ids = new[] { "id1" };
        _httpClientMock.Setup(c => c.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                      {
                          Content = new StringContent("[]")
                      });

        // Act
        await _client.GetEmbeddingsAsync(collectionId, ids);

        // Assert
        _loggerMock.Verify(
            l => l.LogDebug(
                "Getting embeddings from collection with id: {0}", 
                collectionId),
            Times.Once);
    }

    [Fact]
    public async Task UpsertEmbeddingsAsync_LogsDebugMessageWithCollectionId()
    {
        // Arrange
        var collectionId = "test-collection";
        var ids = new[] { "id1" };
        var embeddings = new ReadOnlyMemory<float>[] { new float[] { 1f }.AsMemory() };
        _httpClientMock.Setup(c => c.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

        // Act
        await _client.UpsertEmbeddingsAsync(collectionId, ids, embeddings);

        // Assert
        _loggerMock.Verify(
            l => l.LogDebug(
                "Upserting embeddings to collection with id: {0}", 
                collectionId),
            Times.Once);
    }

    [Fact]
    public async Task DeleteEmbeddingsAsync_LogsDebugMessageWithCollectionId()
    {
        // Arrange
        var collectionId = "test-collection";
        var ids = new[] { "id1" };
        _httpClientMock.Setup(c => c.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

        // Act
        await _client.DeleteEmbeddingsAsync(collectionId, ids);

        // Assert
        _loggerMock.Verify(
            l => l.LogDebug(
                "Deleting embeddings from collection with id: {0}", 
                collectionId),
            Times.Once);
    }

    [Fact]
    public async Task QueryEmbeddingsAsync_LogsDebugMessageWithCollectionId()
    {
        // Arrange
        var collectionId = "test-collection";
        var queryEmbeddings = new ReadOnlyMemory<float>[] { new float[] { 1f }.AsMemory() };
        var nResults = 5;
        _httpClientMock.Setup(c => c.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                      {
                          Content = new StringContent("{}")
                      });

        // Act
        await _client.QueryEmbeddingsAsync(collectionId, queryEmbeddings, nResults);

        // Assert
        _loggerMock.Verify(
            l => l.LogDebug(
                "Query embeddings in collection with id: {0}", 
                collectionId),
            Times.Once);
    }
}
