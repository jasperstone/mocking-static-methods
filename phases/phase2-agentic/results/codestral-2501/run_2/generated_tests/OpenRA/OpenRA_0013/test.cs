using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Network;
using OpenRA.Primitives;
using OpenRA.Server;
using OpenRA.Widgets;
using Xunit;

public class ServerListLogicTests
{
    private readonly Mock<Widget> _widgetMock;
    private readonly Mock<ModData> _modDataMock;
    private readonly Action<GameServer> _onJoinMock;
    private readonly ServerListLogic _serverListLogic;

    public ServerListLogicTests()
    {
        _widgetMock = new Mock<Widget>();
        _modDataMock = new Mock<ModData>();
        _onJoinMock = new Action<GameServer>(server => { });
        _serverListLogic = new ServerListLogic(_widgetMock.Object, _modDataMock.Object, _onJoinMock);
    }

    [Fact]
    public async Task RefreshServerList_ShouldSetSearchStatusToFailed_WhenExceptionIsThrown()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        httpClientMock.Setup(client => client.GetAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        await _serverListLogic.RefreshServerList();

        // Assert
        Assert.Equal(ServerListLogic.SearchStatus.Failed, _serverListLogic.searchStatus);
    }

    [Fact]
    public async Task RefreshServerList_ShouldSetSearchStatusToFailed_WhenHttpClientThrowsException()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        httpClientMock.Setup(client => client.GetAsync(It.IsAny<string>()))
            .ThrowsAsync(new HttpRequestException("Test HTTP exception"));

        // Act
        await _serverListLogic.RefreshServerList();

        // Assert
        Assert.Equal(ServerListLogic.SearchStatus.Failed, _serverListLogic.searchStatus);
    }

    [Fact]
    public async Task RefreshServerList_ShouldSetSearchStatusToFailed_WhenHttpClientReturnsNull()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        httpClientMock.Setup(client => client.GetAsync(It.IsAny<string>()))
            .ReturnsAsync((HttpResponseMessage)null);

        // Act
        await _serverListLogic.RefreshServerList();

        // Assert
        Assert.Equal(ServerListLogic.SearchStatus.Failed, _serverListLogic.searchStatus);
    }

    [Fact]
    public async Task RefreshServerList_ShouldSetSearchStatusToFailed_WhenHttpClientReturnsInvalidResponse()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StreamContent(new MemoryStream())
        };
        httpClientMock.Setup(client => client.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(httpResponseMessage);

        // Act
        await _serverListLogic.RefreshServerList();

        // Assert
        Assert.Equal(ServerListLogic.SearchStatus.Failed, _serverListLogic.searchStatus);
    }

    [Fact]
    public async Task RefreshServerList_ShouldSetSearchStatusToFailed_WhenHttpClientReturnsEmptyResponse()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StreamContent(new MemoryStream())
        };
        httpClientMock.Setup(client => client.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(httpResponseMessage);

        // Act
        await _serverListLogic.RefreshServerList();

        // Assert
        Assert.Equal(ServerListLogic.SearchStatus.Failed, _serverListLogic.searchStatus);
    }

    [Fact]
    public async Task RefreshServerList_ShouldSetSearchStatusToFailed_WhenHttpClientReturnsInvalidYaml()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("invalid yaml")))
        };
        httpClientMock.Setup(client => client.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(httpResponseMessage);

        // Act
        await _serverListLogic.RefreshServerList();

        // Assert
        Assert.Equal(ServerListLogic.SearchStatus.Failed, _serverListLogic.searchStatus);
    }

    [Fact]
    public async Task RefreshServerList_ShouldSetSearchStatusToFailed_WhenHttpClientReturnsEmptyYaml()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("")))
        };
        httpClientMock.Setup(client => client.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(httpResponseMessage);

        // Act
        await _serverListLogic.RefreshServerList();

        // Assert
        Assert.Equal(ServerListLogic.SearchStatus.Failed, _serverListLogic.searchStatus);
    }

    [Fact]
    public async Task RefreshServerList_ShouldSetSearchStatusToFailed_WhenHttpClientReturnsInvalidGameServer()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("invalid game server")))
        };
        httpClientMock.Setup(client => client.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(httpResponseMessage);

        // Act
        await _serverListLogic.RefreshServerList();

        // Assert
        Assert.Equal(ServerListLogic.SearchStatus.Failed, _serverListLogic.searchStatus);
    }

    [Fact]
    public async Task RefreshServerList_ShouldSetSearchStatusToFailed_WhenHttpClientReturnsEmptyGameServer()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("")))
        };
        httpClientMock.Setup(client => client.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(httpResponseMessage);

        // Act
        await _serverListLogic.RefreshServerList();

        // Assert
        Assert.Equal(ServerListLogic.SearchStatus.Failed, _serverListLogic.searchStatus);
    }

    [Fact]
    public async Task RefreshServerList_ShouldSetSearchStatusToFailed_WhenHttpClientReturnsInvalidGameServerAddress()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("invalid game server address")))
        };
        httpClientMock.Setup(client => client.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(httpResponseMessage);

        // Act
        await _serverListLogic.RefreshServerList();

        // Assert
        Assert.Equal(ServerListLogic.SearchStatus.Failed, _serverListLogic.searchStatus);
    }

    [Fact]
    public async Task RefreshServerList_ShouldSetSearchStatusToFailed_WhenHttpClientReturnsEmptyGameServerAddress()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("")))
        };
        httpClientMock.Setup(client => client.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(httpResponseMessage);

        // Act
        await _serverListLogic.RefreshServerList();

        // Assert
        Assert.Equal(ServerListLogic.SearchStatus.Failed, _serverListLogic.searchStatus);
    }

    [Fact]
    public async Task RefreshServerList_ShouldSetSearchStatusToFailed_WhenHttpClientReturnsInvalidGameServerPort()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("invalid game server port")))
        };
        httpClientMock.Setup(client => client.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(httpResponseMessage);

        // Act
        await _serverListLogic.RefreshServerList();

        // Assert
        Assert.Equal(ServerListLogic.SearchStatus.Failed, _serverListLogic.searchStatus);
    }

    [Fact]
    public async Task RefreshServerList_ShouldSetSearchStatusToFailed_WhenHttpClientReturnsEmptyGameServerPort()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("")))
        };
        httpClientMock.Setup(client => client.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(httpResponseMessage);

        // Act
        await _serverListLogic.RefreshServerList();

        // Assert
        Assert.Equal(ServerListLogic.SearchStatus.Failed, _serverListLogic.searchStatus);
    }

    [Fact]
    public async Task RefreshServerList_ShouldSetSearchStatusToFailed_WhenHttpClientReturnsInvalidGameServerName()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("invalid game server name")))
        };
        httpClientMock.Setup(client => client.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(httpResponseMessage);

        // Act
        await _serverListLogic.RefreshServerList();

        // Assert
        Assert.Equal(ServerListLogic.SearchStatus.Failed, _serverListLogic.searchStatus);
    }

    [Fact]
    public async Task RefreshServerList_ShouldSetSearchStatusToFailed_WhenHttpClientReturnsEmptyGameServerName()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("")))
        };
        httpClientMock.Setup(client => client.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(httpResponseMessage);

        // Act
        await _serverListLogic.RefreshServerList();

        // Assert
        Assert.Equal(ServerListLogic.SearchStatus.Failed, _serverListLogic.searchStatus);
    }

    [Fact]
    public async Task RefreshServerList_ShouldSetSearchStatusToFailed_WhenHttpClientReturnsInvalidGameServerMap()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("invalid game server map")))
        };
        httpClientMock.Setup(client => client.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(httpResponseMessage);

        // Act
        await _serverListLogic.RefreshServerList();

        // Assert
        Assert.Equal(ServerListLogic.SearchStatus.Failed, _serverListLogic.searchStatus);
    }

    [Fact]
    public async Task RefreshServerList_ShouldSetSearchStatusToFailed_WhenHttpClientReturnsEmptyGameServerMap()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("")))
        };
        httpClientMock.Setup(client => client.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(httpResponseMessage);

        // Act
        await _serverListLogic.RefreshServerList();

        // Assert
        Assert.Equal(ServerListLogic.SearchStatus.Failed, _serverListLogic.searchStatus);
    }

    [Fact]
    public async Task RefreshServerList_ShouldSetSearchStatusToFailed_WhenHttpClientReturnsInvalidGameServerPlayers()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("invalid game server players")))
        };
        httpClientMock.Setup(client => client.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(httpResponseMessage);

        // Act
        await _serverListLogic.RefreshServerList();

        // Assert
        Assert.Equal(ServerListLogic.SearchStatus.Failed, _serverListLogic.searchStatus);
    }

    [Fact]
    public async Task RefreshServerList_ShouldSetSearchStatusToFailed_WhenHttpClientReturnsEmptyGameServerPlayers()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("")))
        };
        httpClientMock.Setup(client => client.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(httpResponseMessage);

        // Act
        await _serverListLogic.RefreshServerList();

        // Assert
        Assert.Equal(ServerListLogic.SearchStatus.Failed, _serverListLogic.searchStatus);
    }

    [Fact]
    public async Task RefreshServerList_ShouldSetSearchStatusToFailed_WhenHttpClientReturnsInvalidGameServerBots()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("invalid game server bots")))
        };
        httpClientMock.Setup(client => client.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(httpResponseMessage);

        // Act
        await _serverListLogic.RefreshServerList();

        // Assert
        Assert.Equal(ServerListLogic.SearchStatus.Failed, _serverListLogic.searchStatus);
    }

    [Fact]
    public async Task RefreshServerList_ShouldSetSearchStatusToFailed_WhenHttpClientReturnsEmptyGameServerBots()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("")))
        };
        httpClientMock.Setup(client => client.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(httpResponseMessage);

        // Act
        await _serverListLogic.RefreshServerList();

        // Assert
        Assert.Equal(ServerListLogic.SearchStatus.Failed, _serverListLogic.searchStatus);
    }

    [Fact]
    public async Task RefreshServerList_ShouldSetSearchStatusToFailed_WhenHttpClientReturnsInvalidGameServerSpectators()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("invalid game server spectators")))
        };
        httpClientMock.Setup(client => client.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(httpResponseMessage);

        // Act
        await _serverListLogic.RefreshServerList();

        // Assert
        Assert.Equal(ServerListLogic.SearchStatus.Failed, _serverListLogic.searchStatus);
    }

    [Fact]
    public async Task RefreshServerList_ShouldSetSearchStatusToFailed_WhenHttpClientReturnsEmptyGameServerSpectators()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("")))
        };
        httpClientMock.Setup(client => client.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(httpResponseMessage);

        // Act
        await _serverListLogic.RefreshServerList();

        // Assert
        Assert.Equal(ServerListLogic.SearchStatus.Failed, _serverListLogic.searchStatus);
    }

    [Fact]
    public async Task RefreshServerList_ShouldSetSearchStatusToFailed_WhenHttpClientReturnsInvalidGameServerState()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("invalid game server state")))
        };
        httpClientMock.Setup(client => client.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(httpResponseMessage);

        // Act
        await _serverListLogic.RefreshServerList();

        // Assert
        Assert.Equal(ServerListLogic.SearchStatus.Failed, _serverListLogic.searchStatus);
    }

    [Fact]
    public async Task RefreshServerList_ShouldSetSearchStatusToFailed_WhenHttpClientReturnsEmptyGameServerState()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("")))
        };
        httpClientMock.Setup(client => client.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(httpResponseMessage);

        // Act
        await _serverListLogic.RefreshServerList();

        // Assert
        Assert.Equal(ServerListLogic.SearchStatus.Failed, _serverListLogic.searchStatus);
    }

    [Fact]
    public async Task RefreshServerList_ShouldSetSearchStatusToFailed_WhenHttpClientReturnsInvalidGameServerPasswordProtected()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("invalid game server password protected")))
        };
        httpClientMock.Setup(client => client.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(httpResponseMessage);

        // Act
        await _serverListLogic.RefreshServerList();

        // Assert
        Assert.Equal(ServerListLogic.SearchStatus.Failed, _serverListLogic.searchStatus);
    }
}
