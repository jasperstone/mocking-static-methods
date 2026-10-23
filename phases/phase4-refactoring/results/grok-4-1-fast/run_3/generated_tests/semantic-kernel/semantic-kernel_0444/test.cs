using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Microsoft.SemanticKernel.Http;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Chroma.UnitTests;

public class ChromaClientTests
{
    [Fact]
    public async Task GetCollectionAsync_LogsError_OnHttpOperationException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ChromaClient>>();
        mockLogger.Setup(x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()));

        var mockHttpClient = new Mock<HttpClient>();
        var httpOperationException = new HttpOperationException("Test operation failed");
        httpOperationException.RequestContent = "test request";
        httpOperationException.ResponseContent = "test response content";

        mockHttpClient
            .Setup(c => c.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(httpOperationException);

        var loggerFactory = Mock.Of<ILoggerFactory>();
        mockLogger.Setup(lf => lf.CreateLogger(typeof(ChromaClient).FullName)).Returns(mockLogger.Object);

        var client = new ChromaClient(mockHttpClient.Object, endpoint: "http://test-endpoint", loggerFactory: loggerFactory);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpOperationException>(() => client.GetCollectionAsync("test-collection"));
        
        mockLogger.Verify(
            x => x.LogError(
                httpOperationException,
                "{Method} {Path} operation failed: {Message}, {Response}",
                It.IsAny<string>(),
                It.IsAny<string>(),
                httpOperationException.Message,
                httpOperationException.ResponseContent),
            Times.Once);
    }
}
