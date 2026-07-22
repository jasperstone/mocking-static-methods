using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using OpenRA.Mods.Common;
using Xunit;

namespace OpenRA.Mods.Common.Tests
{
    public class ItchIntegrationTests
    {
        [Fact]
        public async Task GetPlayerName_ValidApiKey_ReturnsPlayerName()
        {
            // Arrange
            var itchIntegration = new ItchIntegration();
            var apiKey = "valid_api_key";
            Environment.SetEnvironmentVariable("ITCHIO_API_KEY", apiKey, EnvironmentVariableTarget.Process);

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"user\":{\"username\":\"player_name\",\"display_name\":\"player_display_name\"}}"),
                });

            var httpClient = new HttpClient(handlerMock.Object);

            // Act
            var callbackInvoked = false;
            string playerName = null;
            itchIntegration.GetPlayerName(name =>
            {
                callbackInvoked = true;
                playerName = name;
            });

            // Assert
            await Task.Delay(1000); // wait for the task to complete
            Assert.True(callbackInvoked);
            Assert.NotNull(playerName);
            Assert.Equal("player_display_name", playerName);
        }

        [Fact]
        public async Task GetPlayerName_InvalidApiKey_DoesNotReturnPlayerName()
        {
            // Arrange
            var itchIntegration = new ItchIntegration();
            var apiKey = "";
            Environment.SetEnvironmentVariable("ITCHIO_API_KEY", apiKey, EnvironmentVariableTarget.Process);

            // Act
            var callbackInvoked = false;
            string playerName = null;
            itchIntegration.GetPlayerName(name =>
            {
                callbackInvoked = true;
                playerName = name;
            });

            // Assert
            await Task.Delay(1000); // wait for the task to complete
            Assert.False(callbackInvoked);
            Assert.Null(playerName);
        }
    }
}
