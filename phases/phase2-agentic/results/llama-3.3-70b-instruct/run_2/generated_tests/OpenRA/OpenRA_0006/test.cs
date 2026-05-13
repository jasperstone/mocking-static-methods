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

            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var httpClientMock = new Mock<HttpClient>();
            var httpClientHandlerMock = new Mock<HttpMessageHandler>();

            httpClientHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"user\":{\"username\":\"test-username\",\"display_name\":\"test-display-name\"}}")
                });

            httpClientMock
                .Setup(h => h.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"user\":{\"username\":\"test-username\",\"display_name\":\"test-display-name\"}}")
                });

            httpClientFactoryMock
                .Setup(h => h.CreateClient())
                .Returns(httpClientMock.Object);

            var itchIntegration = new ItchIntegration();

            // Act
            var playerName = "";
            itchIntegration.GetPlayerName(name => playerName = name);
            await Task.Delay(100); // Wait for the task to complete

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
            itchIntegration.GetPlayerName(name => { });
            await Task.Delay(100); // Wait for the task to complete
        }

        [Fact]
        public async Task GetPlayerName_ApiCallFails_DoesNotThrow()
        {
            // Arrange
            var apiKey = "valid-api-key";
            Environment.SetEnvironmentVariable("ITCHIO_API_KEY", apiKey, EnvironmentVariableTarget.Process);

            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var httpClientMock = new Mock<HttpClient>();
            var httpClientHandlerMock = new Mock<HttpMessageHandler>();

            httpClientHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Throws(new HttpRequestException());

            httpClientMock
                .Setup(h => h.GetAsync(It.IsAny<string>()))
                .Throws(new HttpRequestException());

            httpClientFactoryMock
                .Setup(h => h.CreateClient())
                .Returns(httpClientMock.Object);

            var itchIntegration = new ItchIntegration();

            // Act and Assert
            itchIntegration.GetPlayerName(name => { });
            await Task.Delay(100); // Wait for the task to complete
        }
    }
}
