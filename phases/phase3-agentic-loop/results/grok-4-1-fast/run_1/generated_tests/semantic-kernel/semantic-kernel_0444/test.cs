using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Moq;
using Moq.Protected;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Chroma.UnitTests;

public class ChromaClientTests
{
    [Fact]
    public async Task ExecuteHttpRequestAsync_LogsError_OnHttpOperationException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ChromaClient>>();
        var loggerFactory = new TestLoggerFactory(mockLogger.Object);
        var httpMessageHandler = new Mock<HttpMessageHandler>();
        
        // Create HttpOperationException by simulating the failure pattern
        httpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Test failure"));

        var httpClient = new HttpClient(httpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var client = new ChromaClient(httpClient, "http://localhost", loggerFactory);

        using var request = new HttpRequestMessage(HttpMethod.Get, "test");

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<HttpRequestException>(
            () => client.ExecuteHttpRequestAsync(request));

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GET") && 
                                              v.ToString().Contains("test") && 
                                              v.ToString().Contains("Test failure")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private class TestLoggerFactory : ILoggerFactory
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
}
