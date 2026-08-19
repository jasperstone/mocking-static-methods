using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Plugins.Web.Brave;
using Xunit;

namespace BraveConnectorTests
{
    public class BraveConnectorTests
    {
        [Fact]
        public async Task SearchAsync_LogsResponseContent()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(lf => lf.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var httpClientMock = new Mock<HttpClient>();
            var braveConnector = new BraveConnector("apiKey", null, loggerFactoryMock.Object);

            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("{\"results\":[{\"title\":\"Test Title\",\"url\":\"https://www.test.com\",\"description\":\"Test Description\"}]}")
            };

            httpClientMock
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            await braveConnector.SearchAsync<string>("query", 1, 0, CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogTrace("Response content received: {Data}", It.IsAny<string>()), Times.Once);
        }
    }
}
