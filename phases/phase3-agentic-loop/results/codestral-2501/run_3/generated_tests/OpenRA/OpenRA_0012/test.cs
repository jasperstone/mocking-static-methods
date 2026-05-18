using System;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Network;
using OpenRA.Widgets;
using Xunit;

public class RegisteredProfileTooltipLogicTests
{
    [Fact]
    public async Task GetAsync_ShouldBeCalled_WhenProfileIsLoaded()
    {
        // Arrange
        var widgetMock = new Mock<Widget>();
        var worldRendererMock = new Mock<WorldRenderer>();
        var modDataMock = new Mock<ModData>();
        var clientMock = new Mock<Session.Client>();
        var httpClientMock = new Mock<HttpClient>();

        var playerDatabaseMock = new Mock<PlayerDatabase>();
        playerDatabaseMock.Setup(p => p.Profile).Returns("http://example.com/");
        modDataMock.Setup(m => m.GetOrCreate<PlayerDatabase>()).Returns(playerDatabaseMock.Object);

        clientMock.Setup(c => c.Fingerprint).Returns("fingerprint");

        var logic = new RegisteredProfileTooltipLogic(widgetMock.Object, worldRendererMock.Object, modDataMock.Object, clientMock.Object);

        // Act
        await Task.Delay(1000); // Wait for the async task to complete

        // Assert
        httpClientMock.Verify(h => h.GetAsync(It.IsAny<string>()), Times.Once);
    }
}
