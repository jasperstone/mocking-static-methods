using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Connectors.Chroma;

public class ChromaClientTests
{
    [Fact]
    public async Task ExecuteHttpRequestAsync_ShouldLogError_WhenHttpOperationExceptionThrown()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var client = new TestChromaClient("http://test", mockLogger.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "test");
        var exception = new HttpOperationException("Error response");
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);

        // Act & Assert
        await Assert.ThrowsAsync<HttpOperationException>(async () =>
        {
            await client.ExecuteHttpRequestAsync(request, CancellationToken.None);
        });
        // Verify that LogError was called with the exception
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("operation failed")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // Derived class to expose the protected method for testing
    private class TestChromaClient : ChromaClient
    {
        public TestChromaClient(string endpoint, ILogger logger) : base(endpoint, null)
        {
            // Replace the logger with the mock
            typeof(ChromaClient).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(this, logger);
        }

        public new async Task<(HttpResponseMessage, string)> ExecuteHttpRequestAsync(HttpRequestMessage request, CancellationToken token)
        {
            return await base.ExecuteHttpRequestAsync(request, token);
        }
    }
}
