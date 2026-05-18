using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Microsoft.SemanticKernel.Http;
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
        var mockLogger = new Mock<ILogger>();
        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

        var mockHandler = new Mock<HttpMessageHandler>();
        var httpOperationException = new HttpOperationException("Test error message")
        {
            ResponseContent = "Error response content"
        };
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())
            .ThrowsAsync(httpOperationException);

        var httpClient = new HttpClient(mockHandler.Object);
        httpClient.BaseAddress = new Uri("http://test/");

        var client = new ChromaClient(httpClient, endpoint: null, loggerFactory: loggerFactory.Object);

        var request = new HttpRequestMessage(HttpMethod.Get, "test-operation");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<HttpOperationException>(
            () => client.ExecuteHttpRequestAsync(request, CancellationToken.None));

        mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<HttpOperationException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
