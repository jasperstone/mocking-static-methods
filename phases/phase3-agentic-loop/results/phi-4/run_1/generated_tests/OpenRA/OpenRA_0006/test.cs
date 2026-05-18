using Moq;
using Moq.Protected;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
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
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new ItchIntegration.Root
                {
                    User = new ItchIntegration.User
                    {
                        DisplayName = "TestUser",
                        Username = "testuser"
                    }
                }))
            };

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var itchIntegration = new ItchIntegration();
            var callback = new System.Action<string>(name => { });

            // Act
            itchIntegration.GetPlayerName(callback);

            // Assert
            await Task.Delay(100); // Give time for the async operation to complete
            Assert.Equal("TestUser", callback.Invoke());
        }

        [Fact]
        public async Task GetPlayerName_WhenApiCallSucceedsAndNoDisplayName_ReturnsUsername()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new ItchIntegration.Root
                {
                    User = new ItchIntegration.User
                    {
                        DisplayName = null,
                        Username = "testuser"
                    }
                }))
            };

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var itchIntegration = new ItchIntegration();
            var callback = new System.Action<string>(name => { });

            // Act
            itchIntegration.GetPlayerName(callback);

            // Assert
            await Task.Delay(100); // Give time for the async operation to complete
            Assert.Equal("testuser", callback.Invoke());
        }

        [Fact]
        public async Task GetPlayerName_WhenApiCallFails_DoesNotInvokeCallback()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException());

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var itchIntegration = new ItchIntegration();
            var callbackInvoked = false;
            var callback = new System.Action<string>(name =>
            {
                callbackInvoked = true;
            });

            // Act
            itchIntegration.GetPlayerName(callback);

            // Assert
            await Task.Delay(100); // Give time for the async operation to complete
            Assert.False(callbackInvoked);
        }
    }
}
