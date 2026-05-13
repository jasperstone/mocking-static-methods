using System;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA.Mods.Common;
using Xunit;

public class WebServicesTests
{
    [Fact]
    public async Task CheckModVersion_ShouldSetModVersionStatusToLatest_WhenResponseIsLatest()
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
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("latest")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);

        var mockGame = new Mock<Game>();
        mockGame.Setup(g => g.RunAfterTick(It.IsAny<Action>())).Callback<Action>(action => action());

        var webServices = new WebServices();
        webServices.SetHttpClient(httpClient);
        webServices.SetGame(mockGame.Object);

        // Act
        webServices.CheckModVersion();
        await Task.Delay(1000); // Wait for the async operation to complete

        // Assert
        Assert.Equal(ModVersionStatus.Latest, webServices.ModVersionStatus);
    }

    [Fact]
    public async Task CheckModVersion_ShouldSetModVersionStatusToOutdated_WhenResponseIsOutdated()
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
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("outdated")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);

        var mockGame = new Mock<Game>();
        mockGame.Setup(g => g.RunAfterTick(It.IsAny<Action>())).Callback<Action>(action => action());

        var webServices = new WebServices();
        webServices.SetHttpClient(httpClient);
        webServices.SetGame(mockGame.Object);

        // Act
        webServices.CheckModVersion();
        await Task.Delay(1000); // Wait for the async operation to complete

        // Assert
        Assert.Equal(ModVersionStatus.Outdated, webServices.ModVersionStatus);
    }

    // Add more tests for other statuses (Unknown, PlaytestAvailable) similarly
}
