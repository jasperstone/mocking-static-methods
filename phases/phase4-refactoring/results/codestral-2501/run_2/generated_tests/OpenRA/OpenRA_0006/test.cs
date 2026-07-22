using Xunit;
using OpenRA.Mods.Common;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using System.Threading;

namespace OpenRA.Mods.Common.Tests
{
    public class ItchIntegrationTests
    {
        [Fact]
        public async Task GetPlayerName_ShouldCallApiAndReturnUsername()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(new
                    {
                        user = new
                        {
                            username = "testuser",
                            display_name = ""
                        }
                    }))
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var itchIntegration = new ItchIntegration();

            var tcs = new TaskCompletionSource<string>();
            itchIntegration.GetPlayerName(name => tcs.SetResult(name));

            // Act
            var result = await tcs.Task;

            // Assert
            Assert.Equal("testuser", result);
        }

        [Fact]
        public async Task GetPlayerName_ShouldCallApiAndReturnDisplayName()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(new
                    {
                        user = new
                        {
                            username = "testuser",
                            display_name = "Test User"
                        }
                    }))
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var itchIntegration = new ItchIntegration();

            var tcs = new TaskCompletionSource<string>();
            itchIntegration.GetPlayerName(name => tcs.SetResult(name));

            // Act
            var result = await tcs.Task;

            // Assert
            Assert.Equal("Test User", result);
        }
    }
}
