using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
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
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<System.Threading.CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                Content = new StreamContent(new MemoryStream())
            })
            .Verifiable();

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);

        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        mockHttpClientFactory.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var mockWidget = new Mock<Widget>();
        var mockWorldRenderer = new Mock<WorldRenderer>();
        var mockModData = new Mock<ModData>();
        var mockClient = new Mock<Session.Client>();

        var playerDatabase = new PlayerDatabase();
        mockModData.Setup(data => data.GetOrCreate<PlayerDatabase>()).Returns(playerDatabase);

        var logic = new RegisteredProfileTooltipLogic(mockWidget.Object, mockWorldRenderer.Object, mockModData.Object, mockClient.Object);

        // Act
        await Task.Delay(1000); // Wait for the async task to complete

        // Assert
        mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Exactly(1),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
            ItExpr.IsAny<System.Threading.CancellationToken>()
        );
    }
}
