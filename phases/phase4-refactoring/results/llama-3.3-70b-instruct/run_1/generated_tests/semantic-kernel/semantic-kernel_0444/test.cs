using Xunit;
using Moq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.SemanticKernel.Connectors.Chroma.Tests
{
    public class ChromaClientTests
    {
        [Fact]
        public async Task CreateCollectionAsync_LogsError_WhenHttpRequestFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ChromaClient>>();
            var httpClientMock = new Mock<HttpClient>();
            var collectionName = "TestCollection";

            var chromaClient = new ChromaClient(httpClientMock.Object, "https://example.com", new LoggerFactory().CreateLogger<ChromaClient>());

            // Act and Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => chromaClient.CreateCollectionAsync(collectionName));
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
