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
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://itch.io/api/1/jwt/me");
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"user\":{\"username\":\"test-username\",\"display_name\":\"Test Display Name\"}}")
            };

            httpClientMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponseMessage);

            httpClientFactoryMock
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClientMock.Object);

            var itchIntegration = new ItchIntegration();
            var callback = new Action<string>(name => Assert.Equal("Test Display Name", name));

            // Act
            await Task.Run(() => itchIntegration.GetPlayerName(callback));

            // Assert
            httpClientMock.Verify(
                _ => _.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task GetPlayerName_InvalidApiKey_DoesNotThrow()
        {
            // Arrange
            Environment.SetEnvironmentVariable("ITCHIO_API_KEY", "", EnvironmentVariableTarget.Process);

            var itchIntegration = new ItchIntegration();
            var callback = new Action<string>(name => { });

            // Act and Assert
            await Task.Run(() => itchIntegration.GetPlayerName(callback));
        }

        [Fact]
        public async Task GetPlayerName_ApiCallFails_DoesNotThrow()
        {
            // Arrange
            var apiKey = "valid-api-key";
            Environment.SetEnvironmentVariable("ITCHIO_API_KEY", apiKey, EnvironmentVariableTarget.Process);

            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var httpClientMock = new Mock<HttpClient>();
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://itch.io/api/1/jwt/me");
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            httpClientMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponseMessage);

            httpClientFactoryMock
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClientMock.Object);

            var itchIntegration = new ItchIntegration();
            var callback = new Action<string>(name => { });

            // Act and Assert
            await Task.Run(() => itchIntegration.GetPlayerName(callback));
        }
    }
}
