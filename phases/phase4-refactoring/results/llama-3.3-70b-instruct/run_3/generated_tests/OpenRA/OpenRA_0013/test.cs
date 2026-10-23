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
            var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            httpClient.Setup(h => h.GetAsync(It.IsAny<string>())).ReturnsAsync(httpResponseMessage);
            httpClientFactory.Setup(h => h.CreateClient()).Returns(httpClient.Object);

            var serverListLogic = new ServerListLogic(null, null, null);
            serverListLogic.services = new WebServices(httpClientFactory.Object);

            // Act
            await serverListLogic.RefreshServerList();

            // Assert
            httpClient.Verify(h => h.GetAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task RefreshServerList_HandlesGetAsyncException()
        {
            // Arrange
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var exception = new HttpRequestException("Test exception");
            httpClient.Setup(h => h.GetAsync(It.IsAny<string>())).Throws(exception);
            httpClientFactory.Setup(h => h.CreateClient()).Returns(httpClient.Object);

            var serverListLogic = new ServerListLogic(null, null, null);
            serverListLogic.services = new WebServices(httpClientFactory.Object);

            // Act and Assert
            await serverListLogic.RefreshServerList();
            Assert.Equal(ServerListLogic.SearchStatus.Failed, serverListLogic.searchStatus);
        }
    }
}
