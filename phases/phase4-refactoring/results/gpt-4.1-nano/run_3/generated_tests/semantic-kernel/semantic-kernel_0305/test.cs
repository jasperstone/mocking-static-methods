using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Core.Tests
{
    public class SessionsPythonPluginTests
    {
        [Fact]
        public async Task ExecuteCodeAsync_Should_LogTrace_When_Called()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var mockHttpClient = new HttpClient(new FakeHttpMessageHandler());
            mockHttpClientFactory.Setup(_ => _.CreateClient()).Returns(mockHttpClient);

            var settings = new SessionsPythonSettings
            {
                Endpoint = new Uri("https://test.endpoint"),
                SanitizeInput = false
            };

            var plugin = new SessionsPythonPlugin(settings, mockHttpClientFactory.Object, loggerFactory: null);
            // Inject the mock logger
            typeof(SessionsPythonPlugin).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(plugin, mockLogger.Object);

            string testCode = "print(\"Hello World\")";

            // Act
            await plugin.ExecuteCodeAsync(testCode);

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Executing Python code:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    // Fake HttpMessageHandler to avoid real HTTP calls
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("{\"Result\":\"Success\"}")
            };
            return Task.FromResult(response);
        }
    }
}
