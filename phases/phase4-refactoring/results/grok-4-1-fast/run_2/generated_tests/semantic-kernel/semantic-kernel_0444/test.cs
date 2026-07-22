using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Microsoft.SemanticKernel.Http;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Chroma.UnitTests;

public class ChromaClientLoggerTests
{
    [Fact]
    public async Task ExecuteHttpRequestAsync_LogsError_OnHttpOperationException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ChromaClient>>();
        var loggerFactory = new LoggerFactory();
        loggerFactory.AddProvider(new MockLoggerProvider(mockLogger.Object));

        var mockHttpClient = new Mock<HttpClient>();
        var httpOperationException = new HttpOperationException("Test failure")
        {
            ResponseContent = "Test response content"
        };

        mockHttpClient
            .Setup(c => c.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(httpOperationException);

        var client = new ChromaClient(mockHttpClient.Object, "http://test-endpoint", loggerFactory);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<HttpOperationException>(
            () => client.CreateCollectionAsync("test", CancellationToken.None));

        mockLogger.Verify(
            l => l.LogError(
                It.IsAny<Exception>(),
                "{Method} {Path} operation failed: {Message}, {Response}",
                It.IsAny<object>(),
                It.IsAny<object>(),
                It.IsAny<object>(),
                It.IsAny<object>()),
            Times.Once);
    }

    private class MockLoggerProvider : ILoggerProvider
    {
        private readonly Mock<ILogger<ChromaClient>> _logger;

        public MockLoggerProvider(Mock<ILogger<ChromaClient>> logger)
        {
            _logger = logger;
        }

        public ILogger<ChromaClient> CreateLogger(string categoryName) => _logger.Object;

        public void Dispose() { }
    }
}
