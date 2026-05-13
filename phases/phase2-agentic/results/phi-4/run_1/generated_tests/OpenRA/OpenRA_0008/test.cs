using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA.Mods.Common;
using Xunit;

public class WebServicesTests
{
    [Fact]
    public async Task CheckModVersion_OutdatedVersion()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("outdated")
        };

        handlerMock
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
               "SendAsync",
               ItExpr.IsAny<HttpRequestMessage>(),
               ItExpr.IsAny<CancellationToken>()
           )
           .ReturnsAsync(response);

        var httpClient = new HttpClient(handlerMock.Object);
        var webServices = new WebServices
        {
            HttpClientFactory = () => httpClient
        };

        // Act
        webServices.CheckModVersion();
        await Task.Delay(100); // Allow async operation to complete

        // Assert
        Assert.Equal(ModVersionStatus.Outdated, webServices.ModVersionStatus);
    }

    [Fact]
    public async Task CheckModVersion_LatestVersion()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("latest")
        };

        handlerMock
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
               "SendAsync",
               ItExpr.IsAny<HttpRequestMessage>(),
               ItExpr.IsAny<CancellationToken>()
           )
           .ReturnsAsync(response);

        var httpClient = new HttpClient(handlerMock.Object);
        var webServices = new WebServices
        {
            HttpClientFactory = () => httpClient
        };

        // Act
        webServices.CheckModVersion();
        await Task.Delay(100); // Allow async operation to complete

        // Assert
        Assert.Equal(ModVersionStatus.Latest, webServices.ModVersionStatus);
    }

    [Fact]
    public async Task CheckModVersion_UnknownVersion()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("unknown")
        };

        handlerMock
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
               "SendAsync",
               ItExpr.IsAny<HttpRequestMessage>(),
               ItExpr.IsAny<CancellationToken>()
           )
           .ReturnsAsync(response);

        var httpClient = new HttpClient(handlerMock.Object);
        var webServices = new WebServices
        {
            HttpClientFactory = () => httpClient
        };

        // Act
        webServices.CheckModVersion();
        await Task.Delay(100); // Allow async operation to complete

        // Assert
        Assert.Equal(ModVersionStatus.Unknown, webServices.ModVersionStatus);
    }

    [Fact]
    public async Task CheckModVersion_PlaytestAvailable()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("playtest")
        };

        handlerMock
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
               "SendAsync",
               ItExpr.IsAny<HttpRequestMessage>(),
               ItExpr.IsAny<CancellationToken>()
           )
           .ReturnsAsync(response);

        var httpClient = new HttpClient(handlerMock.Object);
        var webServices = new WebServices
        {
            HttpClientFactory = () => httpClient
        };

        // Act
        webServices.CheckModVersion();
        await Task.Delay(100); // Allow async operation to complete

        // Assert
        Assert.Equal(ModVersionStatus.PlaytestAvailable, webServices.ModVersionStatus);
    }
}
