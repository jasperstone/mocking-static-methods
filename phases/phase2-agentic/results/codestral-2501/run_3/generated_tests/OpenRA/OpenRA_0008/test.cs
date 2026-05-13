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
        var webServices = new WebServices();

        // Act
        await webServices.CheckModVersion();

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
        var webServices = new WebServices();

        // Act
        await webServices.CheckModVersion();

        // Assert
        Assert.Equal(ModVersionStatus.Outdated, webServices.ModVersionStatus);
    }

    [Fact]
    public async Task CheckModVersion_ShouldSetModVersionStatusToUnknown_WhenResponseIsUnknown()
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
                Content = new StringContent("unknown")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var webServices = new WebServices();

        // Act
        await webServices.CheckModVersion();

        // Assert
        Assert.Equal(ModVersionStatus.Unknown, webServices.ModVersionStatus);
    }

    [Fact]
    public async Task CheckModVersion_ShouldSetModVersionStatusToPlaytestAvailable_WhenResponseIsPlaytest()
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
                Content = new StringContent("playtest")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var webServices = new WebServices();

        // Act
        await webServices.CheckModVersion();

        // Assert
        Assert.Equal(ModVersionStatus.PlaytestAvailable, webServices.ModVersionStatus);
    }
}
