using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Web.Brave.Tests
{
    public class BraveConnectorTests
    {
        [Fact]
        public async Task SearchAsync_LogsTraceWithResponseContent()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<BraveConnector>>();
            var httpClientMock = new Mock<HttpClient>();
            var braveConnector = new BraveConnector("testApiKey", httpClientMock.Object, loggerFactory: new LoggerFactory().AddProvider(new MockProvider(mockLogger.Object)));

            var responseMessageMock = new Mock<HttpResponseMessage>();
            responseMessageMock.Setup(r => r.Content.ReadAsStringWithExceptionMappingAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync("{\"Web\": {\"Results\": []}}");

            httpClientMock.Setup(c => c.SendWithSuccessCheckAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMessageMock.Object);

            // Act
            await braveConnector.SearchAsync<string>("test query");

            // Assert
            mockLogger.Verify(
                logger => logger.LogTrace(
                    It.Is<string>(s => s.Contains("Response content received:")),
                    It.IsAny<string>()
                ),
                Times.Once
            );
        }
    }

    public class MockProvider : ILoggerProvider
    {
        private readonly Mock<ILogger> _logger;

        public MockProvider(Mock<ILogger> logger)
        {
            _logger = logger;
        }

        public ILogger CreateLogger(string categoryName) => _logger.Object;

        public void Dispose() { }
    }
}
