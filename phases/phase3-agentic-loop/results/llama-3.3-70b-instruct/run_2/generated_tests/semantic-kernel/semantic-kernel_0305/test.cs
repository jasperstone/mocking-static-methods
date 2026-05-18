using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net;
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
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var plugin = new SessionsPythonPlugin(settings, httpClientFactoryMock.Object, null, loggerFactoryMock.Object);

            var httpClientMock = new Mock<HttpClient>();
            var httpResponseMessageMock = new Mock<HttpResponseMessage>();
            httpResponseMessageMock.SetupGet(x => x.IsSuccessStatusCode).Returns(true);
            httpClientMock
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(httpResponseMessageMock.Object);

            httpClientFactoryMock.Setup(x => x.CreateClient()).Returns(httpClientMock.Object);

            // Act
            await plugin.ExecuteCodeAsync("print('Hello World')");

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Executing Python code: print('Hello World')"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteCodeAsync_ThrowsArgumentException_WhenCodeIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionsPythonPlugin>>();
            var settings = new SessionsPythonSettings("sessionId", new Uri("https://example.com"));
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var plugin = new SessionsPythonPlugin(settings, httpClientFactoryMock.Object, null, loggerFactoryMock.Object);

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentException>(() => plugin.ExecuteCodeAsync(null));
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
