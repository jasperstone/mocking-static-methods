using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;

namespace Microsoft.SemanticKernel.Plugins.Core.CodeInterpreter.Tests
{
    public class SessionsPythonPluginTests
    {
        [Fact]
        public async Task ExecuteCodeAsync_LogsTrace()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var settings = new SessionsPythonSettings("some_session_id", new Uri("https://example.com"));
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var httpClientMock = new Mock<HttpClient>();
            var httpResponseMessageMock = new Mock<HttpResponseMessage>();
            httpClientMock.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>())).ReturnsAsync(httpResponseMessageMock.Object);
            httpClientFactoryMock.Setup(x => x.CreateClient()).Returns(httpClientMock.Object);
            var plugin = new SessionsPythonPlugin(
                settings,
                httpClientFactoryMock.Object,
                null,
                loggerFactoryMock.Object);

            // Act
            await plugin.ExecuteCodeAsync("print('Hello World')");

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
