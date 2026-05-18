using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Microsoft.SemanticKernel.Connectors.Chroma.Tests")]

namespace Microsoft.SemanticKernel.Connectors.Chroma.Tests
{
    public class ChromaClientTests
    {
        [Fact]
        public async Task ExecuteHttpRequestAsync_LogsErrorOnHttpOperationException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var httpClientMock = new Mock<HttpClient>();
            var chromaClient = new ChromaClient(httpClientMock.Object, null, loggerMock.Object);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var httpOperationException = new HttpOperationException(null, "Test error", "Response content", null);

            httpClientMock
                .Setup(x => x.SendWithSuccessCheckAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(httpOperationException);

            // Act
            await Assert.ThrowsAsync<HttpOperationException>(() => chromaClient.ExecuteHttpRequestAsync(request));

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<HttpOperationException>(),
                    It.Is<string>(s => s.Contains("{Method} {Path} operation failed: {Message}, {Response}")),
                    It.IsAny<HttpMethod>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ),
                Times.Once
            );
        }
    }
}
