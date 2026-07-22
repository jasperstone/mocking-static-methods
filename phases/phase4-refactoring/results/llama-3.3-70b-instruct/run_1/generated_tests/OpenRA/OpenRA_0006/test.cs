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
            var apiKey = "valid-api-key";
            Environment.SetEnvironmentVariable("ITCHIO_API_KEY", apiKey, EnvironmentVariableTarget.Process);

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"user\":{\"username\":\"test-username\",\"display_name\":\"test-display-name\"}}"),
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var itchIntegration = new ItchIntegration();

            // Act
            var playerName = await GetPlayerNameAsync(itchIntegration, httpClient);

            // Assert
            Assert.Equal("test-display-name", playerName);
        }

        private async Task<string> GetPlayerNameAsync(ItchIntegration itchIntegration, HttpClient httpClient)
        {
            var playerName = string.Empty;
            itchIntegration.GetPlayerName(name => playerName = name);
            await Task.Delay(100); // Wait for the task to complete
            return playerName;
        }
    }
}
