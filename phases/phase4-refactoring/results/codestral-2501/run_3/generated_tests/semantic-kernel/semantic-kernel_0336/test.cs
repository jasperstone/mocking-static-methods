using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Plugins.Web.Brave;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Web.Brave.Tests
{
    public class BraveConnectorTests
    {
        [Fact]
        public async Task SearchAsync_LogsTrace()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<BraveConnector>>();
            var mockHttpClient = new Mock<HttpClient>();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpClient.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    Content = new StringContent("{\"web\":{\"results\":[{\"description\":\"Test description\"}]}}")
                });

            var connector = new BraveConnector("apiKey", mockHttpClient.Object, null, new LoggerFactory());

            // Act
            await connector.SearchAsync<string>("test query");

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Response content received: {Data}")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
