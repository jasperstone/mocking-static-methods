using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Plugins.Core.CodeInterpreter;
using Moq;
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
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var httpClientMock = new Mock<HttpClient>();
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"status\":\"Succeeded\",\"result\":{\"stdout\":\"Hello, World!\"}}")
            };

            httpClientMock.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(httpResponseMessage);

            httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClientMock.Object);

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
    }
}
