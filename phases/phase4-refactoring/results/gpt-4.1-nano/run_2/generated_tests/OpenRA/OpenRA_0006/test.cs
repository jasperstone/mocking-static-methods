using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Moq;
using OpenRA.Mods.Common;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace OpenRA.Tests
{
    public class ItchIntegrationTests
    {
        [Fact]
        public void GetPlayerName_CallsGetAsync_WithCorrectUrl_WhenApiKeyIsSet()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var responseContent = new StringContent("{\"user\": {\"username\": \"testuser\", \"display_name\": \"Test User\"}}");
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK) { Content = responseContent };
            mockHttpMessageHandler
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns<HttpRequestMessage>(async (req) =>
                {
                    return await Task.FromResult(responseMessage);
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            var factoryMock = new Mock<IHttpClientFactory>();
            factoryMock.Setup(f => f.Create()).Returns(client);

            // Set environment variable
            Environment.SetEnvironmentVariable("ITCHIO_API_KEY", "dummy_api_key");

            var integration = new ItchIntegration();

            // Act
            string capturedName = null;
            integration.GetPlayerName(name => capturedName = name);

            // Wait for async task to complete
            Task.Delay(100).Wait();

            // Assert
            mockHttpMessageHandler.Verify(m => m.Send(It.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Get &&
                req.RequestUri == new Uri("https://itch.io/api/1/jwt/me")
            )), Times.Once);

            Assert.Equal("Test User", capturedName);
        }
    }
}
