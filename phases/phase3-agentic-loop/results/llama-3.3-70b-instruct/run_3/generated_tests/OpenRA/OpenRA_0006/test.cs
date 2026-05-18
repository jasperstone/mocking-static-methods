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
                    Content = new StringContent("{\"user\":{\"username\":\"test-username\",\"display_name\":\"test-display-name\"}}")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var itchIntegration = new ItchIntegration();

            // Act
            var playerName = "";
            await Task.Run(async () =>
            {
                await Task.Yield();
                itchIntegration.GetPlayerName(name => playerName = name);
                await Task.Delay(100); // wait for the task to complete
            });

            // Assert
            Assert.Equal("test-display-name", playerName);
        }

        [Fact]
        public async Task GetPlayerName_InvalidApiKey_DoesNotThrow()
        {
            // Arrange
            Environment.SetEnvironmentVariable("ITCHIO_API_KEY", "", EnvironmentVariableTarget.Process);

            var itchIntegration = new ItchIntegration();

            // Act and Assert
            await Task.Run(async () =>
            {
                await Task.Yield();
                itchIntegration.GetPlayerName(name => { });
                await Task.Delay(100); // wait for the task to complete
            });
        }

        [Fact]
        public async Task GetPlayerName_ApiCallFails_DoesNotThrow()
        {
            // Arrange
            var apiKey = "valid-api-key";
            Environment.SetEnvironmentVariable("ITCHIO_API_KEY", apiKey, EnvironmentVariableTarget.Process);

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .Throws(new HttpRequestException());

            var httpClient = new HttpClient(handlerMock.Object);
            var itchIntegration = new ItchIntegration();

            // Act and Assert
            await Task.Run(async () =>
            {
                await Task.Yield();
                itchIntegration.GetPlayerName(name => { });
                await Task.Delay(100); // wait for the task to complete
            });
        }
    }
}
