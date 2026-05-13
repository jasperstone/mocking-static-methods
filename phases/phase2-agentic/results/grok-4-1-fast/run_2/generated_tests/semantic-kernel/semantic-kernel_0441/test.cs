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
    private const string TestCollectionId = "test-collection-id";
    private const string TestEndpoint = "http://localhost:8000";

    [Fact]
    public async Task GetEmbeddingsAsync_LogsDebugMessage_Coverage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ChromaClient>>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(typeof(ChromaClient))).Returns(loggerMock.Object);

        var httpClientMock = new Mock<HttpClient>();

        var client = new ChromaClient(httpClientMock.Object, TestEndpoint, loggerFactoryMock.Object);

        // Act
        await client.GetEmbeddingsAsync(TestCollectionId, new[] { "id1" });

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Debug,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Getting embeddings from collection with id: test-collection-id")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task UpsertEmbeddingsAsync_LogsDebugMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ChromaClient>>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(typeof(ChromaClient))).Returns(loggerMock.Object);

        var httpClientMock = new Mock<HttpClient>();

        var client = new ChromaClient(httpClientMock.Object, TestEndpoint, loggerFactoryMock.Object);

        // Act
        await client.UpsertEmbeddingsAsync(TestCollectionId, new[] { "id1" }, new[] { new ReadOnlyMemory<float>(new float[] { 1f }) });

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Debug,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Upserting embeddings to collection with id: test-collection-id")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteEmbeddingsAsync_LogsDebugMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ChromaClient>>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(typeof(ChromaClient))).Returns(loggerMock.Object);

        var httpClientMock = new Mock<HttpClient>();

        var client = new ChromaClient(httpClientMock.Object, TestEndpoint, loggerFactoryMock.Object);

        // Act
        await client.DeleteEmbeddingsAsync(TestCollectionId, new[] { "id1" });

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Debug,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Deleting embeddings from collection with id: test-collection-id")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task QueryEmbeddingsAsync_LogsDebugMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ChromaClient>>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(typeof(ChromaClient))).Returns(loggerMock.Object);

        var httpClientMock = new Mock<HttpClient>();

        var client = new ChromaClient(httpClientMock.Object, TestEndpoint, loggerFactoryMock.Object);

        // Act
        await client.QueryEmbeddingsAsync(TestCollectionId, new[] { new ReadOnlyMemory<float>(new float[] { 1f }) }, 5);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Debug,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Query embeddings in collection with id: test-collection-id")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ListCollectionsAsync_LogsDebugMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ChromaClient>>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(typeof(ChromaClient))).Returns(loggerMock.Object);

        var httpClientMock = new Mock<HttpClient>();

        var client = new ChromaClient(httpClientMock.Object, TestEndpoint, loggerFactoryMock.Object);

        // Act
        await foreach (var _ in client.ListCollectionsAsync())
        {
            // Consume the async enumerable
        }

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Debug,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Listing collections")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateCollectionAsync_LogsDebugMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ChromaClient>>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(typeof(ChromaClient))).Returns(loggerMock.Object);

        var httpClientMock = new Mock<HttpClient>();

        var client = new ChromaClient(httpClientMock.Object, TestEndpoint, loggerFactoryMock.Object);

        // Act
        await client.CreateCollectionAsync("test-collection");

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Debug,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Creating collection test-collection")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCollectionAsync_LogsDebugMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ChromaClient>>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(typeof(ChromaClient))).Returns(loggerMock.Object);

        var httpClientMock = new Mock<HttpClient>();

        var client = new ChromaClient(httpClientMock.Object, TestEndpoint, loggerFactoryMock.Object);

        // Act
        await client.GetCollectionAsync("test-collection");

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Debug,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Getting collection test-collection")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteCollectionAsync_LogsDebugMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ChromaClient>>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(typeof(ChromaClient))).Returns(loggerMock.Object);

        var httpClientMock = new Mock<HttpClient>();

        var client = new ChromaClient(httpClientMock.Object, TestEndpoint, loggerFactoryMock.Object);

        // Act
        await client.DeleteCollectionAsync("test-collection");

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Debug,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Deleting collection test-collection")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_UsesNullLogger_WhenLoggerFactoryIsNull()
    {
        // Act
        var client = new ChromaClient(TestEndpoint);

        // Assert
        Assert.IsType<NullLogger>(client.GetType().GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(client));
    }
}
