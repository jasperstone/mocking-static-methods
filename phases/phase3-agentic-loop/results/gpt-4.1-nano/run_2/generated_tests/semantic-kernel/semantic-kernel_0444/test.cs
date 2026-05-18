using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Http;
using Microsoft.SemanticKernel.Connectors.Chroma;

namespace ChromaClientTests
{
    public class ChromaClientLoggingTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly HttpClient _httpClient;

        public ChromaClientLoggingTests()
        {
            _loggerMock = new Mock<ILogger>();
            _httpClient = new HttpClient(); // Not used directly in the test, but needed for constructor
        }

        [Fact]
        public async Task ExecuteHttpRequestAsync_Should_LogError_When_HttpOperationExceptionThrown()
        {
            // Arrange
            var client = new TestChromaClient(_loggerMock.Object, _httpClient);
            var request = new HttpRequestMessage(HttpMethod.Get, "test");
            var exception = new HttpOperationException("Error response content");

            // Act & Assert
            await Assert.ThrowsAsync<HttpOperationException>(() => client.ExecuteHttpRequestAsync(request));

            // Verify that LogError was called with the expected message
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("operation failed")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        // Helper class to override the method for testing
        private class TestChromaClient : ChromaClient
        {
            private readonly HttpRequestMessage _testRequest;
            private readonly Exception _throwException;

            public TestChromaClient(ILogger logger, HttpClient httpClient)
                : base("http://test", null)
            {
                _logger = logger;
                _httpClient = httpClient;
                _testRequest = new HttpRequestMessage(HttpMethod.Get, "test");
                _throwException = new HttpOperationException("Error response content");
            }

            public override async Task<(HttpResponseMessage, string)> ExecuteHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
            {
                throw _throwException;
            }
        }
    }
}
