using Xunit;
using OpenRA.Mods.Common;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenRA.Support;

namespace OpenRA.Mods.Common.Tests
{
    public class ItchIntegrationTests
    {
        [Fact]
        public void GetPlayerName_ValidApiKey_ReturnsDisplayName()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new ItchIntegration.Root
                {
                    User = new ItchIntegration.User
                    {
                        DisplayName = "TestDisplayName",
                        Username = "TestUsername"
                    }
                }))
            };
            mockHttpMessageHandler
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(response);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                DefaultRequestHeaders = { Authorization = new AuthenticationHeaderValue("Bearer", "fake_api_key") }
            };

            var itchIntegration = new ItchIntegration();
            var callbackInvoked = false;
            string returnedName = null;

            // Act
            itchIntegration.GetPlayerName(name =>
            {
                callbackInvoked = true;
                returnedName = name;
            });

            // Assert
            Assert.True(callbackInvoked);
            Assert.Equal("TestDisplayName", returnedName);
        }

        [Fact]
        public void GetPlayerName_ValidApiKeyNoDisplayName_ReturnsUsername()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new ItchIntegration.Root
                {
                    User = new ItchIntegration.User
                    {
                        Username = "TestUsername"
                    }
                }))
            };
            mockHttpMessageHandler
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(response);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                DefaultRequestHeaders = { Authorization = new AuthenticationHeaderValue("Bearer", "fake_api_key") }
            };

            var itchIntegration = new ItchIntegration();
            var callbackInvoked = false;
            string returnedName = null;

            // Act
            itchIntegration.GetPlayerName(name =>
            {
                callbackInvoked = true;
                returnedName = name;
            });

            // Assert
            Assert.True(callbackInvoked);
            Assert.Equal("TestUsername", returnedName);
        }

        [Fact]
        public void GetPlayerName_InvalidApiKey_DoesNotReturnName()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            mockHttpMessageHandler
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(response);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                DefaultRequestHeaders = { Authorization = new AuthenticationHeaderValue("Bearer", "fake_api_key") }
            };

            var itchIntegration = new ItchIntegration();
            var callbackInvoked = false;
            string returnedName = null;

            // Act
            itchIntegration.GetPlayerName(name =>
            {
                callbackInvoked = true;
                returnedName = name;
            });

            // Assert
            Assert.False(callbackInvoked);
            Assert.Null(returnedName);
        }
    }
}
