using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;
using OpenRA.Mods.Common;

namespace OpenRA.Tests
{
    public class ItchIntegrationTests
    {
        [Fact]
        public async Task GetPlayerName_WhenApiCallSucceeds_ReturnsDisplayName()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("{\"user\":{\"display_name\":\"TestUser\",\"username\":\"testuser\"}}")
            };

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>()
                )
                .ReturnsAsync(response);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://itch.io/")
            };

            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock
                .Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var itchIntegration = new ItchIntegration
            {
                HttpClientFactory = httpClientFactoryMock.Object
            };

            var callback = new System.Action<string>(name => { });

            // Act
            itchIntegration.GetPlayerName(callback);

            // Assert
            await Task.Delay(100); // Give some time for the async operation to complete
            Assert.Equal("TestUser", callback.Invoke());
        }
    }
}
