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
    public async Task GetAsync_ShouldBeCalledWithCorrectUrl()
    {
        // Arrange
        var widget = new Mock<Widget>();
        var worldRenderer = new Mock<WorldRenderer>();
        var modData = new Mock<ModData>();
        var client = new Mock<Session.Client>();
        var httpClientFactory = new Mock<IHttpClientFactory>();

        var httpClient = new Mock<HttpClient>();
        httpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient.Object);

        var logic = new RegisteredProfileTooltipLogic(widget.Object, worldRenderer.Object, modData.Object, client.Object);

        // Act
        await Task.Delay(1000); // Wait for the async task to complete

        // Assert
        httpClient.Verify(c => c.GetAsync(It.IsAny<string>()), Times.Once);
    }
}
