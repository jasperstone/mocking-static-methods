using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.SemanticKernel.Http;

namespace Microsoft.SemanticKernel.Connectors.Chroma.Tests
{
    public class ChromaClientTests
    {
        [Fact]
        public async Task ExecuteHttpRequestAsync_LogsError_WhenExceptionIsThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var httpClientMock = new Mock<HttpClient>();

            var chromaClient = new ChromaClient(httpClientMock.Object, "https://example.com", new LoggerFactory().CreateLogger<ChromaClient>());

            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api/v1/test");

            httpClientMock
                .Setup(h => h.SendAsync(request, It.IsAny<CancellationToken>()))
                .Throws(new Exception("Test exception"));

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => chromaClient.ExecuteHttpRequestAsync(request));

            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
