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
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Setup<Task<HttpResponseMessage>>(
                    h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            var httpClient = new HttpClient(handlerMock.Object);
            var serverListLogic = new ServerListLogic(null, null, null);

            // Act
            await Task.Run(async () =>
            {
                serverListLogic.RefreshServerList();
            });

            // Assert
            handlerMock.Verify(
                h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
    }
}
