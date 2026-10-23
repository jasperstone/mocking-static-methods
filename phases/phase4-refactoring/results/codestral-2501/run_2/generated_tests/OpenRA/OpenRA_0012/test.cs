using System;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Graphics;
using OpenRA.Network;
using OpenRA.Widgets;
using Xunit;

public class RegisteredProfileTooltipLogicTests
{
    [Fact]
    public async Task GetAsync_ShouldBeCalledWithCorrectUrl()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(mockHttpMessageHandler.Object);

        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        mockHttpClientFactory.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var widget = new Widget();
        var worldRenderer = new WorldRenderer();
        var modData = new ModData();
        var client = new Session.Client { Fingerprint = "testFingerprint" };

        var playerDatabase = new PlayerDatabase { Profile = "http://example.com/profile/" };
        modData.Add(playerDatabase);

        var logic = new RegisteredProfileTooltipLogic(widget, worldRenderer, modData, client);

        // Act
        await Task.Delay(1000); // Wait for the async task to complete

        // Assert
        mockHttpClientFactory.Verify(factory => factory.CreateClient(It.IsAny<string>()), Times.Once);
        mockHttpMessageHandler.Verify(
            handler => handler.SendAsync(It.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == "http://example.com/profile/testFingerprint"), It.IsAny<System.Threading.CancellationToken>()),
            Times.Once);
    }
}
