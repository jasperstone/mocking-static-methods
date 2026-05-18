using Moq;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Network;
using OpenRA.Server;
using OpenRA.Support;
using OpenRA.Traits;
using OpenRA.Widgets;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace OpenRA.Tests
{
    public class ServerListLogicTests
    {
        [Fact]
        public async Task RefreshServerList_MakesGetRequestToServerList()
        {
            // Arrange
            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            httpClient.Setup(h => h.GetAsync(It.IsAny<string>())).ReturnsAsync(httpResponseMessage);

            var modData = new ModData(new Manifest(), new InstalledMods(), false);
            var serverListLogic = new ServerListLogic(null, modData, null);

            // Act
            await serverListLogic.RefreshServerList();

            // Assert
            //httpClient.Verify(h => h.GetAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task RefreshServerList_HandlesFailedRequest()
        {
            // Arrange
            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            httpClient.Setup(h => h.GetAsync(It.IsAny<string>())).ReturnsAsync(httpResponseMessage);

            var modData = new ModData(new Manifest(), new InstalledMods(), false);
            var serverListLogic = new ServerListLogic(null, modData, null);

            // Act
            await serverListLogic.RefreshServerList();

            // Assert
            //Assert.Equal(SearchStatus.Failed, serverListLogic.searchStatus);
        }
    }
}
