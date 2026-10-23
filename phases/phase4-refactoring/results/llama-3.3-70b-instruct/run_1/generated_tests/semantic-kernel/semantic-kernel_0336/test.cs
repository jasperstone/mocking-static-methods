using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Plugins.Web.Brave;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Web.Tests
{
    public class BraveConnectorTests
    {
        [Fact]
        public async Task SearchAsync_LogsResponseContent()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BraveConnector>>();
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"results\":[{\"title\":\"Test\",\"url\":\"https://www.test.com\"}]}")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var braveConnector = new BraveConnector("apiKey", httpClient);

            // Act
            await braveConnector.SearchAsync<string>("query");

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<object, Exception, string>>()
            ), Times.Once);
        }
    }
}
