using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Chroma.UnitTests;

public class ChromaClientTests
{
    private const string CollectionId = "test-collection";
    private static readonly string[] Ids = { "id1", "id2" };

    private Mock<HttpClient> CreateMockHttpClient()
    {
        var httpClient = new Mock<HttpClient>();
        httpClient.Setup(c => c.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("[]")
            });
        return httpClient;
    }

    [Fact]
    public async Task GetEmbeddingsAsync_LogsDebugMessageWithCollectionId()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var httpClient = CreateMockHttpClient();
        var client = new ChromaClient(httpClient.Object, endpoint: "http://localhost", loggerFactory: new TestLoggerFactory(mockLogger.Object));

        mockLogger
            .Setup(l => l.Log(
                It.Is<LogLevel>(l => l == LogLevel.Debug),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Getting embeddings from collection with id") && v.ToString()!.Contains(CollectionId)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Verifiable();

        // Act
        await client.GetEmbeddingsAsync(CollectionId, Ids);

        // Assert
        mockLogger.Verify();
    }

    [Fact]
    public async Task UpsertEmbeddingsAsync_LogsDebugMessageWithCollectionId()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var httpClient = CreateMockHttpClient();
        var client = new ChromaClient(httpClient.Object, endpoint: "http://localhost", loggerFactory: new TestLoggerFactory(mockLogger.Object));

        mockLogger
            .Setup(l => l.Log(
                It.Is<LogLevel>(l => l == LogLevel.Debug),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Upserting embeddings to collection with id") && v.ToString()!.Contains(CollectionId)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Verifiable();

        var embeddings = Array.Empty<ReadOnlyMemory<float>>();

        // Act
        await client.UpsertEmbeddingsAsync(CollectionId, Ids, embeddings);

        // Assert
        mockLogger.Verify();
    }

    [Fact]
    public async Task DeleteEmbeddingsAsync_LogsDebugMessageWithCollectionId()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var httpClient = CreateMockHttpClient();
        var client = new ChromaClient(httpClient.Object, endpoint: "http://localhost", loggerFactory: new TestLoggerFactory(mockLogger.Object));

        mockLogger
            .Setup(l => l.Log(
                It.Is<LogLevel>(l => l == LogLevel.Debug),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Deleting embeddings from collection with id") && v.ToString()!.Contains(CollectionId)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Verifiable();

        // Act
        await client.DeleteEmbeddingsAsync(CollectionId, Ids);

        // Assert
        mockLogger.Verify();
    }

    [Fact]
    public async Task QueryEmbeddingsAsync_LogsDebugMessageWithCollectionId()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var httpClient = CreateMockHttpClient();
        var client = new ChromaClient(httpClient.Object, endpoint: "http://localhost", loggerFactory: new TestLoggerFactory(mockLogger.Object));

        mockLogger
            .Setup(l => l.Log(
                It.Is<LogLevel>(l => l == LogLevel.Debug),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Query embeddings in collection with id") && v.ToString()!.Contains(CollectionId)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Verifiable();

        var queryEmbeddings = Array.Empty<ReadOnlyMemory<float>>();

        // Act
        await client.QueryEmbeddingsAsync(CollectionId, queryEmbeddings, nResults: 5);

        // Assert
        mockLogger.Verify();
    }

    [Fact]
    public async Task ListCollectionsAsync_LogsDebugMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var httpClient = CreateMockHttpClient();
        var client = new ChromaClient(httpClient.Object, endpoint: "http://localhost", loggerFactory: new TestLoggerFactory(mockLogger.Object));

        mockLogger
            .Setup(l => l.Log(
                It.Is<LogLevel>(l => l == LogLevel.Debug),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Listing collections")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Verifiable();

        // Act
        await foreach (var _ in client.ListCollectionsAsync())
        {
        }

        // Assert
        mockLogger.Verify();
    }
}

public class TestLoggerFactory : ILoggerFactory
{
    private readonly ILogger _logger;

    public TestLoggerFactory(ILogger logger)
    {
        _logger = logger;
    }

    public void Dispose() { }

    public void AddProvider(ILoggerProvider provider) { }

    public ILogger CreateLogger(string categoryName) => _logger;
}
