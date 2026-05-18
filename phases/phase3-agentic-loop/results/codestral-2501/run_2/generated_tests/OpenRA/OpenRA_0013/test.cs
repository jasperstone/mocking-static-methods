using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Network;
using OpenRA.Server;
using OpenRA.Widgets;
using Xunit;

public class ServerListLogicTests
{
    [Fact]
    public void ProgressLabelText_ReturnsCorrectStatusMessages()
    {
        // Arrange
        var mockWidget = new Mock<Widget>();
        var mockModData = new Mock<ModData>();
        var mockOnJoin = new Mock<Action<GameServer>>();
        var serverListLogic = new ServerListLogic(mockWidget.Object, mockModData.Object, mockOnJoin.Object);

        // Act & Assert
        serverListLogic.searchStatus = ServerListLogic.SearchStatus.Failed;
        Assert.Equal("label-search-status-failed", serverListLogic.ProgressLabelText());

        serverListLogic.searchStatus = ServerListLogic.SearchStatus.NoGames;
        Assert.Equal("label-search-status-no-games", serverListLogic.ProgressLabelText());

        serverListLogic.searchStatus = ServerListLogic.SearchStatus.Fetching;
        Assert.Equal("", serverListLogic.ProgressLabelText());
    }

    [Fact]
    public async Task RefreshServerList_HandlesQueryCorrectly()
    {
        // Arrange
        var mockWidget = new Mock<Widget>();
        var mockModData = new Mock<ModData>();
        var mockOnJoin = new Mock<Action<GameServer>>();
        var mockHttpClient = new Mock<HttpClient>();
        var mockHttpResponseMessage = new Mock<HttpResponseMessage>();
        var mockContent = new Mock<HttpContent>();

        mockHttpClient.Setup(c => c.GetAsync(It.IsAny<string>())).ReturnsAsync(mockHttpResponseMessage.Object);
        mockHttpResponseMessage.Setup(r => r.Content).Returns(mockContent.Object);
        mockContent.Setup(c => c.ReadAsStreamAsync()).ReturnsAsync(new System.IO.MemoryStream());

        var mockHttpClientFactory = new Mock<HttpClientFactory>();
        mockHttpClientFactory.Setup(f => f.Create()).Returns(mockHttpClient.Object);

        var serverListLogic = new ServerListLogic(mockWidget.Object, mockModData.Object, mockOnJoin.Object);

        // Act
        serverListLogic.RefreshServerList();
        await Task.Delay(100); // Wait for the async operation to complete

        // Assert
        Assert.Equal(ServerListLogic.SearchStatus.Failed, serverListLogic.searchStatus);
    }
}
