using Xunit;
using Moq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace Duplicati.Tests
{
    public class JsonWebHelperHttpClientTests
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _httpClient;
        private readonly JsonWebHelperHttpClient _helper;

        public JsonWebHelperHttpClientTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _httpClient = new HttpClient(_handlerMock.Object);
            _helper = new JsonWebHelperHttpClient(_httpClient);
        }

        [Fact]
        public async Task GetResponseUncheckedAsync_CallsSendAsync_ReturnsResponse()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            _handlerMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _helper.GetResponseUncheckedAsync(request, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None);

            // Assert
            Assert.Equal(responseMessage, result);
            _handlerMock.Verify(m => m.SendAsync(It.Is<HttpRequestMessage>(r => r == request), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ReadJsonResponseAsync_DeserializesJsonResponse()
        {
            // Arrange
            var expectedObject = new { Value = "test" };
            var json = JsonConvert.SerializeObject(expectedObject);
            var responseContent = new StringContent(json, Encoding.UTF8, "application/json");
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK) { Content = responseContent };

            _handlerMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMessage);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");

            // Act
            var result = await _helper.ReadJsonResponseAsync<dynamic>(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test", (string)result.Value);
        }
    }
}
