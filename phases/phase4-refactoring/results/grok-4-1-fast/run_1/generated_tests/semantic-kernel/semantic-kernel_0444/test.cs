using System;
using System.Net.Http;
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
    [Fact]
    public async Task CreateCollectionAsync_LogsError_OnHttpOperationException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ChromaClient>>();
        mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        
        var loggerFactory = new LoggerFactory();
        loggerFactory.AddProvider(new MockLoggerProvider(mockLogger.Object));

        var mockHttpClient = new Mock<HttpClient>();
        mockHttpClient.Setup(c => c.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network failure"));

        var client = new ChromaClient(mockHttpClient.Object, endpoint: "http://localhost", loggerFactory: loggerFactory);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => client.CreateCollectionAsync("test"));

        mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("POST") && v.ToString()!.Contains("api/v1")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
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
