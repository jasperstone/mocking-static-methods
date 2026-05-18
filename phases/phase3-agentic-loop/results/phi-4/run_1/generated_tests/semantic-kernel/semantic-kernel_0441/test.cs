using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Chroma.Tests
{
    public class ChromaClientTests
    {
        [Fact]
        public async Task GetEmbeddingsAsync_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new MockLoggerProvider(loggerMock.Object));
            var chromaClient = new ChromaClient("http://localhost", loggerFactory);

            // Act
            await chromaClient.GetEmbeddingsAsync("testCollectionId", new string[] { "id1", "id2" }, null, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.LogDebug(
                    It.Is<string>(s => s.Contains("Getting embeddings from collection with id: testCollectionId")),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }

    public class MockLoggerProvider : ILoggerProvider
    {
        private readonly ILogger _logger;

        public MockLoggerProvider(ILogger logger)
        {
            _logger = logger;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _logger;
        }

        public void Dispose()
        {
        }
    }
}
