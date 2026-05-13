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
        public async Task SearchAsync_CallsLogTrace_WithExpectedParameters()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BraveConnector>>();
            var httpClientMock = new Mock<HttpClient>();
            var responseMessageMock = new Mock<HttpResponseMessage>();
            responseMessageMock.Setup(r => r.Content.ReadAsStringWithExceptionMappingAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync("{\"Web\": {\"Results\": []}}");

            var connector = new BraveConnector("testApiKey", httpClientMock.Object, logger: loggerMock.Object);

            // Act
            await connector.SearchAsync<string>("test query", 1, 0, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                l => l.LogTrace(
                    It.IsAny<LogLevel>(),
                    It.Is<EventId>(e => e.Id == 0),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Response content received: {Data}")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
