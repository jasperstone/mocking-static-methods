using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Chroma.Tests
{
    public class ChromaClientTests
    {
        [Fact]
        public async Task ExecuteHttpRequestAsync_LogsError_WhenHttpRequestFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ChromaClient>>();
            var httpClientMock = new Mock<HttpClient>();
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var httpOperationException = new HttpOperationException("Test error message", new HttpResponseMessage(HttpStatusCode.BadRequest));

            var chromaClient = new ChromaClient(httpClientMock.Object, "https://example.com", new LoggerFactory().CreateLogger<ChromaClient>());

            // Act and Assert
            await Assert.ThrowsAsync<HttpOperationException>(() => chromaClient.ExecuteHttpRequestAsync(httpRequestMessage));
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
