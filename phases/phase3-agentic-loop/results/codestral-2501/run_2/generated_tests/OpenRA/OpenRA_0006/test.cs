using Xunit;
using Moq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using OpenRA.Mods.Common;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using OpenRA.Support;

namespace OpenRA.Mods.Common.Tests
{
    public class ItchIntegrationTests
    {
        [Fact]
        public async Task GetPlayerName_ShouldCallApiAndReturnDisplayName()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(new ItchIntegration.Root
                    {
                        User = new ItchIntegration.User
                        {
                            DisplayName = "TestDisplayName",
                            Username = "TestUsername"
                        }
                    }))
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            HttpClientFactory.SetHttpClient(httpClient);

            var itchIntegration = new ItchIntegration();
            var callbackCalled = false;
            string returnedName = null;

            // Act
            itchIntegration.GetPlayerName(name =>
            {
                returnedName = name;
                callbackCalled = true;
            });

            await Task.Delay(1000); // Wait for the async operation to complete

            // Assert
            Assert.True(callbackCalled);
            Assert.Equal("TestDisplayName", returnedName);
        }

        [Fact]
        public async Task GetPlayerName_ShouldReturnUsernameWhenDisplayNameIsEmpty()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(new ItchIntegration.Root
                    {
                        User = new ItchIntegration.User
                        {
                            DisplayName = "",
                            Username = "TestUsername"
                        }
                    }))
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            HttpClientFactory.SetHttpClient(httpClient);

            var itchIntegration = new ItchIntegration();
            var callbackCalled = false;
            string returnedName = null;

            // Act
            itchIntegration.GetPlayerName(name =>
            {
                returnedName = name;
                callbackCalled = true;
            });

            await Task.Delay(1000); // Wait for the async operation to complete

            // Assert
            Assert.True(callbackCalled);
            Assert.Equal("TestUsername", returnedName);
        }

        [Fact]
        public async Task GetPlayerName_ShouldLogErrorWhenApiCallFails()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            HttpClientFactory.SetHttpClient(httpClient);

            var itchIntegration = new ItchIntegration();
            var callbackCalled = false;
            string returnedName = null;

            // Act
            itchIntegration.GetPlayerName(name =>
            {
                returnedName = name;
                callbackCalled = true;
            });

            await Task.Delay(1000); // Wait for the async operation to complete

            // Assert
            Assert.False(callbackCalled);
            Assert.Null(returnedName);
        }
    }
}
