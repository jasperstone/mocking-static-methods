using System;
using System.Net.Http;
using System.Text;
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
    private const string TestCollectionId = "test-collection";
    private static readonly string[] TestIds = { "id1", "id2" };

    [Fact]
    public async Task GetEmbeddingsAsync_LogsDebugMessage_WithCollectionId()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger
            .Setup(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains($"Getting embeddings from collection with id: {TestCollectionId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Verifiable();

        var httpClient = new Mock<HttpClient>();
        var fakeResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("{\"ids\":[],\"embeddings\":[],\"metadatas\":[]}")
        };
        httpClient.Setup(c => c.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(fakeResponse);

        // Create logger factory that returns our mock logger
        var loggerFactory = new LoggerFactory();
        loggerFactory.AddProvider(new MockLoggerProvider(mockLogger.Object));
        
        var client = new ChromaClient(httpClient.Object, "http://localhost", loggerFactory);

        // Act
        await client.GetEmbeddingsAsync(TestCollectionId, TestIds);

        // Assert
        mockLogger.Verify();
    }

    private class MockLoggerProvider : ILoggerProvider
    {
        private readonly ILogger _logger;

        public MockLoggerProvider(ILogger logger)
        {
            _logger = logger;
        }

        public ILogger CreateLogger(string categoryName) => _logger;

        public void Dispose() { }
    }
}
