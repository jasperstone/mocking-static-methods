using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA.Mods.Common.Widgets.Logic;
using Xunit;

namespace OpenRA.Tests
{
    public class ServerListLogicTests
    {
        [Fact]
        public async Task RefreshServerList_SuccessfulResponse()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("game1:address1\ngame2:address2")
            };

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>()
                )
                .ReturnsAsync(response);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var serverListLogic = new ServerListLogic(null, null, null);
            serverListLogic.services = new Mock<WebServices>().Object;
            serverListLogic.services.ServerList = "http://example.com";

            // Act
            await serverListLogic.RefreshServerList();

            // Assert
            Assert.Equal(SearchStatus.NoGames, serverListLogic.searchStatus);
            Assert.NotEmpty(serverListLogic.games);
        }

        [Fact]
        public async Task RefreshServerList_ExceptionThrown()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException());

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var serverListLogic = new ServerListLogic(null, null, null);
            serverListLogic.services = new Mock<WebServices>().Object;
            serverListLogic.services.ServerList = "http://example.com";

            // Act
            await serverListLogic.RefreshServerList();

            // Assert
            Assert.Equal(SearchStatus.Failed, serverListLogic.searchStatus);
        }
    }
}
