using Moq;
using OpenRA.Mods.Common;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace OpenRA.Mods.Common.Tests
{
    public class ItchIntegrationTests
    {
        [Fact]
        public async Task GetPlayerName_CallsGetAsync_OnHttpClient()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"user\":{\"username\":\"testuser\",\"display_name\":\"Test User\"}}"),
            };

            handlerMock
                .Setup<Task<HttpResponseMessage>>(
                    h => h.SendAsync(
                        It.IsAny<HttpRequestMessage>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(response);

            var httpClient = new HttpClient(handlerMock.Object);
            var itchIntegration = new ItchIntegration();
            var callback = new Action<string>(name => { });

            Environment.SetEnvironmentVariable("ITCHIO_API_KEY", "test_api_key");

            // Act
            await Task.Run(() => itchIntegration.GetPlayerName(callback));

            // Assert
            handlerMock
                .Verify(
                    h => h.SendAsync(
                        It.IsAny<HttpRequestMessage>(),
                        It.IsAny<CancellationToken>()
                    ),
                    Times.Once()
                );
        }
    }
}
