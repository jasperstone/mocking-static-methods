using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using OpenRA;
using Xunit;

public class MapPreviewTests
{
    [Fact]
    public async Task Install_ShouldSetDownloadErrorStatus_WhenGetAsyncFails()
    {
        // Arrange
        var modData = new ModData();
        var mapPreview = new MapPreview(modData, "uid", MapGridType.Rectangular, new MapCache());
        var mockHttpClient = new Mock<HttpClient>();
        mockHttpClient.Setup(client => client.GetAsync(It.IsAny<string>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));

        // Act
        await mapPreview.Install("http://example.com", mockHttpClient.Object);

        // Assert
        Assert.Equal(MapStatus.DownloadError, mapPreview.Status);
    }

    [Fact]
    public async Task Install_ShouldSetDownloadErrorStatus_WhenMapFilenameIsEmpty()
    {
        // Arrange
        var modData = new ModData();
        var mapPreview = new MapPreview(modData, "uid", MapGridType.Rectangular, new MapCache());
        var mockHttpClient = new Mock<HttpClient>();
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        response.Content = new StringContent("");
        response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment") { FileName = "" };
        mockHttpClient.Setup(client => client.GetAsync(It.IsAny<string>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(response);

        // Act
        await mapPreview.Install("http://example.com", mockHttpClient.Object);

        // Assert
        Assert.Equal(MapStatus.DownloadError, mapPreview.Status);
    }

    [Fact]
    public async Task Install_ShouldSetDownloadErrorStatus_WhenMapInstallPackageIsNull()
    {
        // Arrange
        var modData = new ModData();
        var mapPreview = new MapPreview(modData, "uid", MapGridType.Rectangular, new MapCache());
        var mockHttpClient = new Mock<HttpClient>();
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        response.Content = new StringContent("");
        response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment") { FileName = "mapfile" };
        mockHttpClient.Setup(client => client.GetAsync(It.IsAny<string>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(response);

        // Act
        await mapPreview.Install("http://example.com", mockHttpClient.Object);

        // Assert
        Assert.Equal(MapStatus.DownloadError, mapPreview.Status);
    }
}
