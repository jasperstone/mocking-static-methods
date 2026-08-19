using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Plugins.Core.CodeInterpreter;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Core.Tests.CodeInterpreter
{
    public class SessionsPythonPluginTests
    {
        [Fact]
        public async Task ExecuteCodeAsync_LogsTrace()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionsPythonPlugin>>();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var httpClientMock = new Mock<HttpClient>();
            httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClientMock.Object);
            var settings = new SessionsPythonSettings("sessionId", new Uri("http://example.com"));
            var plugin = new SessionsPythonPlugin(settings, httpClientFactoryMock.Object, loggerFactory: new NullLoggerFactory());

            var code = "print('Hello, World!')";

            // Act
            await plugin.ExecuteCodeAsync(code, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Executing Python code: print('Hello, World!')")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteCodeAsync_HandlesNullCode()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionsPythonPlugin>>();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var settings = new SessionsPythonSettings("sessionId", new Uri("http://example.com"));
            var plugin = new SessionsPythonPlugin(settings, httpClientFactoryMock.Object, loggerFactory: new NullLoggerFactory());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => plugin.ExecuteCodeAsync(null, CancellationToken.None));
        }

        [Fact]
        public async Task ExecuteCodeAsync_HandlesEmptyCode()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionsPythonPlugin>>();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var settings = new SessionsPythonSettings("sessionId", new Uri("http://example.com"));
            var plugin = new SessionsPythonPlugin(settings, httpClientFactoryMock.Object, loggerFactory: new NullLoggerFactory());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => plugin.ExecuteCodeAsync(string.Empty, CancellationToken.None));
        }

        [Fact]
        public async Task ExecuteCodeAsync_HandlesWhitespaceCode()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionsPythonPlugin>>();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var settings = new SessionsPythonSettings("sessionId", new Uri("http://example.com"));
            var plugin = new SessionsPythonPlugin(settings, httpClientFactoryMock.Object, loggerFactory: new NullLoggerFactory());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => plugin.ExecuteCodeAsync("   ", CancellationToken.None));
        }

        [Fact]
        public async Task ExecuteCodeAsync_HandlesSanitizedInput()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionsPythonPlugin>>();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var settings = new SessionsPythonSettings("sessionId", new Uri("http://example.com"))
            {
                SanitizeInput = true
            };
            var plugin = new SessionsPythonPlugin(settings, httpClientFactoryMock.Object, loggerFactory: new NullLoggerFactory());

            var code = "print('Hello, World!')";

            // Act
            await plugin.ExecuteCodeAsync(code, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Executing Python code: print('Hello, World!')")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteCodeAsync_HandlesHttpRequestException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionsPythonPlugin>>();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var httpClientMock = new Mock<HttpClient>();
            httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClientMock.Object);
            httpClientMock.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("Mocked exception"));
            var settings = new SessionsPythonSettings("sessionId", new Uri("http://example.com"));
            var plugin = new SessionsPythonPlugin(settings, httpClientFactoryMock.Object, loggerFactory: new NullLoggerFactory());

            var code = "print('Hello, World!')";

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => plugin.ExecuteCodeAsync(code, CancellationToken.None));
        }
    }
}
