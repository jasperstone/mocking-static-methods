using Xunit;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Plugins.Core.CodeInterpreter;

namespace SemanticKernel.Tests
{
    public class SessionsPythonPluginTests
    {
        [Fact]
        public async Task ExecuteCodeAsync_LogsTrace_WhenCodeIsProvided()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var settings = new SessionsPythonSettings("sessionId", new Uri("https://example.com"));
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var plugin = new SessionsPythonPlugin(settings, httpClientFactoryMock.Object, null, new LoggerFactory().CreateLogger<SessionsPythonPlugin>());

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
        public async Task ExecuteCodeAsync_ThrowsArgumentNullException_WhenCodeIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var settings = new SessionsPythonSettings("sessionId", new Uri("https://example.com"));
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var plugin = new SessionsPythonPlugin(settings, httpClientFactoryMock.Object, null, new LoggerFactory().CreateLogger<SessionsPythonPlugin>());

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => plugin.ExecuteCodeAsync(null));
        }

        [Fact]
        public async Task ExecuteCodeAsync_ThrowsArgumentNullException_WhenCodeIsEmpty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var settings = new SessionsPythonSettings("sessionId", new Uri("https://example.com"));
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var plugin = new SessionsPythonPlugin(settings, httpClientFactoryMock.Object, null, new LoggerFactory().CreateLogger<SessionsPythonPlugin>());

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => plugin.ExecuteCodeAsync(string.Empty));
        }
    }
}
