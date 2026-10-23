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
    public async Task GetAsync_Called_With_Correct_Url()
    {
        // Arrange
        var widget = new Mock<Widget>();
        var worldRenderer = new Mock<WorldRenderer>();
        var modData = new Mock<ModData>();
        var client = new Mock<Session.Client>();
        var httpClient = new Mock<HttpClient>();

        var playerDatabase = new Mock<PlayerDatabase>();
        playerDatabase.Setup(p => p.Profile).Returns("http://example.com/profile/");
        client.Setup(c => c.Fingerprint).Returns("12345");

        modData.Setup(m => m.GetOrCreate<PlayerDatabase>()).Returns(playerDatabase.Object);

        var logic = new RegisteredProfileTooltipLogic(widget.Object, worldRenderer.Object, modData.Object, client.Object);

        // Act
        await Task.Delay(1000); // Wait for the async task to complete

        // Assert
        httpClient.Verify(h => h.GetAsync("http://example.com/profile/12345"), Times.Once);
    }
}
