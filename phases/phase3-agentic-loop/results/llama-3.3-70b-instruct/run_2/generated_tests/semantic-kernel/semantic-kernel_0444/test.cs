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
        public async Task ExecuteHttpRequestAsync_LogsErrorOnHttpOperationException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var httpClientMock = new Mock<HttpClient>();
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var httpOperationException = new Exception("Test error message");

            var chromaClient = new ChromaClient(httpClientMock.Object, "https://example.com");
            var fieldInfo = typeof(ChromaClient).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fieldInfo.SetValue(chromaClient, loggerMock.Object);

            httpClientMock
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .Throws(httpOperationException);

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => chromaClient.ExecuteHttpRequestAsync(httpRequestMessage));
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
