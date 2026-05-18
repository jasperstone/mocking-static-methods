using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Plugins.Core.CodeInterpreter;

namespace PluginTests
{
    public class SessionsPythonPluginTests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly SessionsPythonSettings _settings;
        private readonly SessionsPythonPlugin _plugin;

        public SessionsPythonPluginTests()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _loggerMock = new Mock<ILogger>();
            _settings = new SessionsPythonSettings
            {
                Endpoint = new Uri("https://testendpoint")
            };

            _plugin = new SessionsPythonPlugin(_settings, _httpClientFactoryMock.Object, null, new LoggerFactory());
        }

        [Fact]
        public async Task ExecuteCodeAsync_LogsTraceCall()
        {
            // Arrange
            var code = "print(\"hello\")";

            var responseContent = new StringContent("{\"result\":\"ok\"}");
            var responseMock = new Mock<HttpResponseMessage>();
            responseMock.Setup(r => r.Content).Returns(responseContent);
            responseMock.Setup(r => r.Content.ReadAsStringAsync()).ReturnsAsync("{\"result\":\"ok\"}");

            var handlerMock = new Moq.Mock<HttpMessageHandler>();
            handlerMock
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns<HttpRequestMessage>(async (req) =>
                {
                    var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                    {
                        Content = new StringContent("{\"result\":\"ok\"}")
                    };
                    return response;
                });

            var client = new HttpClient(handlerMock.Object);
            _httpClientFactoryMock.Setup(f => f.CreateClient()).Returns(client);

            // Act
            var result = await _plugin.ExecuteCodeAsync(code);

            // Assert
            _loggerMock.Verify(
                x => x.LogTrace(It.Is<string>(s => s.Contains("Executing Python code")), It.IsAny<object[]>()),
                Times.Once);
            Assert.NotNull(result);
        }
    }
}
