using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;

namespace Microsoft.SemanticKernel.Plugins.Core.CodeInterpreter.Tests
{
    public class SessionsPythonPluginTests
    {
        [Fact]
        public async Task ExecuteCodeAsync_LogsTrace()
        {
            // Arrange
            var loggerFactory = new Mock<ILoggerFactory>();
            var logger = new Mock<ILogger<SessionsPythonPlugin>>();
            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            var settings = new SessionsPythonSettings("sessionId", new Uri("https://example.com"));
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var handlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(handlerMock.Object);
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"result\":\"success\"}")
            };
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(httpResponseMessage);
            httpClientFactory.Setup(x => x.CreateClient()).Returns(httpClient);
            var plugin = new SessionsPythonPlugin(settings, httpClientFactory.Object, null, loggerFactory.Object);

            // Act
            await plugin.ExecuteCodeAsync("print('Hello World')");

            // Assert
            logger.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }
    }
}
