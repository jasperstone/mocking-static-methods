using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA.Game.Map;
using Xunit;

public class MapPreviewTests
{
    private class MockMapPreview : MapPreview
    {
        public MockMapPreview(HttpClient client, string baseUri, string uid) : base(client, baseUri, uid)
        {
        }

        public override MapStatus Status { get; set; }
    }

    [Fact]
    public async Task Install_SuccessfulDownload()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("dummy content"),
                ContentHeaders = { ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment") { FileName = "mapfile.map" } }
            });

        var client = new HttpClient(mockHttpMessageHandler.Object);
        var mapPreview = new MockMapPreview(client, "http://example.com/maps/", "mapUid");

        // Act
        await mapPreview.Install("http://example.com/maps/");

        // Assert
        Assert.Equal(MapStatus.Downloaded, mapPreview.Status);
    }

    [Fact]
    public async Task Install_FailedDownload()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        var client = new HttpClient(mockHttpMessageHandler.Object);
        var mapPreview = new MockMapPreview(client, "http://example.com/maps/", "mapUid");

        // Act
        await mapPreview.Install("http://example.com/maps/");

        // Assert
        Assert.Equal(MapStatus.DownloadError, mapPreview.Status);
    }
}
