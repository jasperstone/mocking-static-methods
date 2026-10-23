using System;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Network;
using OpenRA.Primitives;
using OpenRA.Support;
using OpenRA.Traits;
using OpenRA.Widgets;
using Xunit;

namespace OpenRA.Mods.Common.Widgets.Logic.Tests
{
    public class ServerListLogicTests
    {
        [Fact]
        public async Task RefreshServerList_MakesGetRequestToServerList()
        {
            // Arrange
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var handler = new Mock<HttpMessageHandler>();
            handler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            httpClient
                .Setup(h => h.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            var serverListLogic = new ServerListLogic(null, null, null);
            var services = new WebServices();
            services.ServerList = "https://example.com/serverlist";
            //serverListLogic.services = services;

            // Act
            await serverListLogic.RefreshServerList();

            // Assert
            handler.Verify(
                h => h.SendAsync(
                    It.IsAny<HttpRequestMessage>(),
                    It.IsAny<CancellationToken>()
                ),
                Times.Once
            );
        }
    }
}
