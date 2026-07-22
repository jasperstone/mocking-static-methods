using Xunit;
using Moq;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.SemanticKernel.Connectors.Chroma
{
    public class ChromaClientTests
    {
        [Fact]
        public async Task ExecuteHttpRequestAsync_LogsErrorOnHttpOperationException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ChromaClient>>();
            var httpClient = new HttpClient();
            var chromaClient = new ChromaClient(httpClient, "https://example.com", new LoggerFactory().CreateLogger<ChromaClient>());

            // Act and Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => chromaClient.ExecuteHttpRequestAsync(new HttpRequestMessage()));
            loggerMock.Verify(
                l => l.LogError(
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()
                ),
                Times.Once
            );
        }
    }
}
