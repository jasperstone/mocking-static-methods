using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Core.CodeInterpreter.Tests
{
    public class SessionsPythonPluginTests
    {
        [Fact]
        public async Task ExecuteCodeAsync_LogsTrace()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionsPythonPlugin>>();
            var settings = new SessionsPythonSettings("sessionId", new Uri("https://example.com"));
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var httpClientMock = new Mock<HttpClient>();
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            var httpClient = new HttpClient(handlerMock.Object);
            httpClientFactoryMock.Setup(x => x.CreateClient()).Returns(httpClient);
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var plugin = new SessionsPythonPlugin(settings, httpClientFactoryMock.Object, null, loggerFactoryMock.Object);

            // Act
            await plugin.ExecuteCodeAsync("print('Hello World')");

            // Assert
            loggerMock.Verify(l => l.LogTrace("Executing Python code: {Code}", "print('Hello World')"), Times.Once);
        }

        [Fact]
        public async Task ExecuteCodeAsync_ThrowsArgumentNullException_WhenCodeIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionsPythonPlugin>>();
            var settings = new SessionsPythonSettings("sessionId", new Uri("https://example.com"));
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var plugin = new SessionsPythonPlugin(settings, httpClientFactoryMock.Object, null, loggerFactoryMock.Object);

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => plugin.ExecuteCodeAsync(null));
        }

        [Fact]
        public async Task ExecuteCodeAsync_ThrowsArgumentException_WhenCodeIsEmpty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionsPythonPlugin>>();
            var settings = new SessionsPythonSettings("sessionId", new Uri("https://example.com"));
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var plugin = new SessionsPythonPlugin(settings, httpClientFactoryMock.Object, null, loggerFactoryMock.Object);

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentException>(() => plugin.ExecuteCodeAsync(string.Empty));
        }
    }
}
