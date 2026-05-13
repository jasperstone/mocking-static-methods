using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Network;
using OpenRA.Utilities;

namespace OpenRA.Tests
{
    public class ServerListLogicTests
    {
        [Fact]
        public async Task RefreshServerList_ValidResponse_ReturnsGameServers()
        {
            // Arrange
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new Mock<HttpResponseMessage>();
            var httpContent = new Mock<HttpContent>();
            var yaml = new MiniYaml();

            var gameServer = new GameServer(new Node
            {
                Value = "address",
                Port = 1234,
                Name = "Test Server",
                Map = "Test Map",
                Mod = "Test Mod",
                Version = "1.0",
                Players = 10,
                MaxPlayers = 20,
                Bots = 5,
                Spectators = 2,
                IsPasswordProtected = false,
                IsJoinable = true
            });

            var yamlString = yaml.Write(gameServer);

            httpContent.Setup(c => c.ReadAsStreamAsync()).ReturnsAsync(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(yamlString)));
            httpResponseMessage.Setup(m => m.Content).Returns(httpContent.Object);
            httpResponseMessage.Setup(m => m.IsSuccessStatusCode).Returns(true);
            httpClient.Setup(c => c.GetAsync(It.IsAny<string>())).ReturnsAsync(httpResponseMessage.Object);
            httpClientFactory.Setup(f => f.CreateClient()).Returns(httpClient.Object);

            var serverListLogic = new ServerListLogic(new Widget(), new ModData(), (gameServer) => { });

            // Act
            await serverListLogic.RefreshServerList();

            // Assert
            Assert.NotNull(serverListLogic.currentServer);
            Assert.Equal(gameServer.Address, serverListLogic.currentServer.Address);
            Assert.Equal(gameServer.Port, serverListLogic.currentServer.Port);
            Assert.Equal(gameServer.Name, serverListLogic.currentServer.Name);
            Assert.Equal(gameServer.Map, serverListLogic.currentServer.Map);
            Assert.Equal(gameServer.Mod, serverListLogic.currentServer.Mod);
            Assert.Equal(gameServer.Version, serverListLogic.currentServer.Version);
            Assert.Equal(gameServer.Players, serverListLogic.currentServer.Players);
            Assert.Equal(gameServer.MaxPlayers, serverListLogic.currentServer.MaxPlayers);
            Assert.Equal(gameServer.Bots, serverListLogic.currentServer.Bots);
            Assert.Equal(gameServer.Spectators, serverListLogic.currentServer.Spectators);
            Assert.Equal(gameServer.IsPasswordProtected, serverListLogic.currentServer.IsPasswordProtected);
            Assert.Equal(gameServer.IsJoinable, serverListLogic.currentServer.IsJoinable);
        }

        [Fact]
        public async Task RefreshServerList_InvalidResponse_ThrowsException()
        {
            // Arrange
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new Mock<HttpResponseMessage>();
            var httpContent = new Mock<HttpContent>();

            httpContent.Setup(c => c.ReadAsStreamAsync()).Throws(new Exception("Test exception"));
            httpResponseMessage.Setup(m => m.Content).Returns(httpContent.Object);
            httpResponseMessage.Setup(m => m.IsSuccessStatusCode).Returns(false);
            httpClient.Setup(c => c.GetAsync(It.IsAny<string>())).ReturnsAsync(httpResponseMessage.Object);
            httpClientFactory.Setup(f => f.CreateClient()).Returns(httpClient.Object);

            var serverListLogic = new ServerListLogic(new Widget(), new ModData(), (gameServer) => { });

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => serverListLogic.RefreshServerList());
        }
    }
}
