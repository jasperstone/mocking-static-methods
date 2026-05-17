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
            var apiKey = "valid-api-key";
            Environment.SetEnvironmentVariable("ITCHIO_API_KEY", apiKey, EnvironmentVariableTarget.Process);

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .SetupTaskAsyncHandler(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"user\":{\"username\":\"test-username\",\"display_name\":\"Test Display Name\"}}")
                });

            var httpClient = new HttpClient(handlerMock.Object);

            // Act
            var playerName = "";
            itchIntegration.GetPlayerName(name => playerName = name);
            await Task.Delay(1000); // wait for the task to complete

            // Assert
            Assert.Equal("Test Display Name", playerName);
        }

        [Fact]
        public async Task GetPlayerName_InvalidApiKey_DoesNotThrow()
        {
            // Arrange
            var itchIntegration = new ItchIntegration();
            Environment.SetEnvironmentVariable("ITCHIO_API_KEY", "", EnvironmentVariableTarget.Process);

            // Act and Assert
            itchIntegration.GetPlayerName(name => { });
            await Task.Delay(1000); // wait for the task to complete
        }

        [Fact]
        public async Task GetPlayerName_ApiCallFails_DoesNotThrow()
        {
            // Arrange
            var itchIntegration = new ItchIntegration();
            var apiKey = "valid-api-key";
            Environment.SetEnvironmentVariable("ITCHIO_API_KEY", apiKey, EnvironmentVariableTarget.Process);

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .SetupTaskAsyncHandler(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            var httpClient = new HttpClient(handlerMock.Object);

            // Act and Assert
            itchIntegration.GetPlayerName(name => { });
            await Task.Delay(1000); // wait for the task to complete
        }
    }
}
