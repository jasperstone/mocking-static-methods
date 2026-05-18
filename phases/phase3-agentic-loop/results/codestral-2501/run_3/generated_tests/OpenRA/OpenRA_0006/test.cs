using Xunit;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using OpenRA.Mods.Common;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;

namespace OpenRA.Mods.Common.Tests
{
    public class ItchIntegrationTests
    {
        [Fact]
        public async Task GetPlayerName_ShouldCallGetAsync()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);

            var itchIntegration = new ItchIntegration();
            var callbackCalled = false;
            Action<string> callback = (name) => callbackCalled = true;

            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new Root
                {
                    User = new User
                    {
                        Username = "testuser",
                        DisplayName = "Test User"
                    }
                }))
            };

            mockHttpMessageHandler.Setup(_ => _.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            itchIntegration.GetPlayerName(callback);

            // Assert
            mockHttpMessageHandler.Verify(
                x => x.SendAsync(It.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri == new Uri("https://itch.io/api/1/jwt/me") &&
                    req.Headers.Authorization.Scheme == "Bearer" &&
                    req.Headers.Authorization.Parameter == "fake_api_key"
                ), It.IsAny<CancellationToken>()),
                Times.Once
            );

            Assert.True(callbackCalled);
        }

        public class User
        {
            public string Url { get; set; }
            public bool Gamer { get; set; }
            public int Id { get; set; }
            public bool PressUser { get; set; }
            public bool Developer { get; set; }
            public string Username { get; set; }
            public string DisplayName { get; set; }
        }

        public class Root
        {
            public User User { get; set; }
        }
    }
}
