using System;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using OpenRA.Mods.Common.Widgets.Logic;
using Xunit;

public class ServerListLogicTests
{
    [Fact]
    public async Task RefreshServerList_ShouldCallGetAsync()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(mockHttpMessageHandler.Object);

        var serverListLogic = new ServerListLogic(null, null, null);

        // Act
        await serverListLogic.RefreshServerList();

        // Assert
        mockHttpMessageHandler.Verify(
            x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<System.Threading.CancellationToken>()),
            Times.Once);
    }
}
