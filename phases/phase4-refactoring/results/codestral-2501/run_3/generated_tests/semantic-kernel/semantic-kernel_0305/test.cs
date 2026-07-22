using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Plugins.Core.CodeInterpreter;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;
using Microsoft.SemanticKernel.Http;

namespace Microsoft.SemanticKernel.Plugins.Core.Tests.CodeInterpreter
{
    public class SessionsPythonPluginTests
    {
        [Fact]
        public async Task ExecuteCodeAsync_LogsTrace()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SessionsPythonPlugin>>();
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var mockHttpClient = new Mock<HttpClient>();

            mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(mockHttpClient.Object);

            var settings = new SessionsPythonSettings("sessionId", new System.Uri("http://example.com"))
            {
                SanitizeInput = false
            };

            var plugin = new SessionsPythonPlugin(settings, mockHttpClientFactory.Object, loggerFactory: Mock.Of<ILoggerFactory>());

            // Mock the SendAsync method to return a successful response
            mockHttpClient
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"result\": \"success\"}")
                });

            // Act
            await plugin.ExecuteCodeAsync("print('Hello, World!')", CancellationToken.None);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Executing Python code: print('Hello, World!')")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
