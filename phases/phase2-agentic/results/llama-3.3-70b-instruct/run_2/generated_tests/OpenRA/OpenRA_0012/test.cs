using Moq;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Network;
using OpenRA.Support;
using OpenRA.Widgets;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace OpenRA.Mods.Common.Tests
{
    public class RegisteredProfileTooltipLogicTests
    {
        [Fact]
        public async Task GetAsync_CalledWithCorrectUrl()
        {
            // Arrange
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
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
            httpClientMock
                .Setup(h => h.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
            var playerDatabase = new PlayerDatabase();
            var client = new Session.Client();
            var widget = new Widget();
            var worldRenderer = new WorldRenderer();
            var modData = new ModData();

            // Act
            var logic = new RegisteredProfileTooltipLogic(widget, worldRenderer, modData, client);
            await Task.Run(async () =>
            {
                try
                {
                    var url = playerDatabase.Profile + client.Fingerprint;
                    var httpResponseMessage = await httpClientMock.Object.GetAsync(url);
                }
                catch (Exception e)
                {
                    throw e;
                }
            });

            // Assert
            httpClientMock.Verify(h => h.GetAsync(It.IsAny<string>()), Times.Once);
        }
    }
}
