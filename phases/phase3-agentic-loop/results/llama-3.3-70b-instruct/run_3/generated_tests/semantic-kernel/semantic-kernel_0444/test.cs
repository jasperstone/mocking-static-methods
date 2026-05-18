using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.SemanticKernel.Connectors.Chroma;

namespace UnitTests
{
    public class ChromaClientTests
    {
        [Fact]
        public async Task ExecuteHttpRequestAsync_LogsError_WhenHttpRequestFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var httpClient = new HttpClient(new MockHttpMessageHandler());
            var chromaClient = new ChromaClient(httpClient, null, new LoggerFactory().CreateLogger<ChromaClient>());

            // Act and Assert
            try
            {
                await chromaClient.ExecuteHttpRequestAsync(new HttpRequestMessage(HttpMethod.Get, "https://example.com/api/v1/"));
            }
            catch (HttpRequestException)
            {
                loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
            }
        }
    }

    public class MockHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromException<HttpResponseMessage>(new HttpRequestException());
        }
    }
}
