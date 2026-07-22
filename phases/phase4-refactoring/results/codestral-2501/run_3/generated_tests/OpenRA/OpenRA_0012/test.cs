using System;
using System.IO;
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
    public async Task TestGetAsyncCall()
    {
        // Arrange
        var widget = new Mock<Widget>();
        var worldRenderer = new Mock<WorldRenderer>();
        var modData = new Mock<ModData>();
        var client = new Mock<Session.Client>();
        var httpClient = new Mock<HttpClient>();

        var playerDatabase = new Mock<PlayerDatabase>();
        playerDatabase.Setup(p => p.Profile).Returns("http://fakeurl.com/");
        modData.Setup(m => m.GetOrCreate<PlayerDatabase>()).Returns(playerDatabase.Object);

        var registeredProfileTooltipLogic = new RegisteredProfileTooltipLogic(widget.Object, worldRenderer.Object, modData.Object, client.Object);

        // Act
        var url = playerDatabase.Object.Profile + client.Object.Fingerprint;
        var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("Player: { ProfileName: 'Test', ProfileRank: 'Rank' }")
        };
        httpClient.Setup(h => h.GetAsync(url)).ReturnsAsync(httpResponseMessage);

        // Assert
        await Task.Delay(1000); // Wait for the async task to complete
        Assert.NotNull(registeredProfileTooltipLogic.profile);
        Assert.Equal("Test", registeredProfileTooltipLogic.profile.ProfileName);
        Assert.Equal("Rank", registeredProfileTooltipLogic.profile.ProfileRank);
    }
}
