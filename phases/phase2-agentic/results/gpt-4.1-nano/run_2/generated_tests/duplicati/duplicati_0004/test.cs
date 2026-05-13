using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using System.Net;
using System.Net.Http.Headers;

namespace Duplicati.Tests
{
    public class JsonWebHelperHttpClientTests
    {
        private readonly Mock<HttpClient> _httpClientMock;
        private readonly JsonWebHelperHttpClient _helper;

        public JsonWebHelperHttpClientTests()
        {
            _httpClientMock = new Mock<HttpClient>();
            _helper = new JsonWebHelperHttpClient(_httpClientMock.Object);
        }

        [Fact]
        public async Task GetResponseUncheckedAsync_CallsSendAsync_ReturnsResponse()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            var responseTask = Task.FromResult(responseMessage);

            var mockHttpClient = new Mock<HttpClient>();
            mockHttpClient
                .Setup(c => c.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .Returns(responseTask);

            var helper = new JsonWebHelperHttpClient(mockHttpClient.Object);

            // Act
            var result = await helper.GetResponseUncheckedAsync(request, CancellationToken.None);

            // Assert
            Assert.Equal(responseMessage, result);
            mockHttpClient.Verify(c => c.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetResponseAsync_SuccessResponse_ReturnsResponse()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            var responseTask = Task.FromResult(responseMessage);

            var mockHttpClient = new Mock<HttpClient>();
            mockHttpClient
                .Setup(c => c.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMessage);

            var helper = new JsonWebHelperHttpClient(mockHttpClient.Object);

            // Act
            var result = await helper.GetResponseAsync(request, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None);

            // Assert
            Assert.Equal(responseMessage, result);
            mockHttpClient.Verify(c => c.SendAsync(It.IsAny<HttpRequestMessage>(), HttpCompletionOption.ResponseHeadersRead, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetJsonDataAsync_ValidResponse_ReturnsDeserializedObject()
        {
            // Arrange
            var testObject = new { Value = "test" };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(testObject);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };

            var mockHttpClient = new Mock<HttpClient>();
            mockHttpClient
                .Setup(c => c.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMessage);

            var helper = new JsonWebHelperHttpClient(mockHttpClient.Object);

            // Act
            var result = await helper.ReadJsonResponseAsync<dynamic>(new HttpRequestMessage(HttpMethod.Get, "http://test"), CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test", (string)result.Value);
        }
    }
}
