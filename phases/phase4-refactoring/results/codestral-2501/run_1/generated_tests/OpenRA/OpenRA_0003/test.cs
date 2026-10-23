using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using OpenRA;
using Xunit;

namespace OpenRA.Tests
{
    public class MapPreviewTests
    {
        [Fact]
        public async Task Install_ShouldSetDownloadErrorStatus_WhenGetAsyncFails()
        {
            // Arrange
            var mockHttpClient = new Mock<HttpClient>();
            mockHttpClient.Setup(client => client.GetAsync(It.IsAny<string>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(mockHttpClient.Object);

            var mapPreview = new MapPreview(new ModData(), "uid", MapGridType.Rectangular, new MapCache());

            // Act
            await mapPreview.Install("http://example.com");

            // Assert
            Assert.Equal(MapStatus.DownloadError, mapPreview.Status);
        }

        [Fact]
        public async Task Install_ShouldSetDownloadErrorStatus_WhenMapFilenameIsEmpty()
        {
            // Arrange
            var mockHttpClient = new Mock<HttpClient>();
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            response.Content = new StringContent("");
            response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment") { FileName = "" };
            mockHttpClient.Setup(client => client.GetAsync(It.IsAny<string>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(mockHttpClient.Object);

            var mapPreview = new MapPreview(new ModData(), "uid", MapGridType.Rectangular, new MapCache());

            // Act
            await mapPreview.Install("http://example.com");

            // Assert
            Assert.Equal(MapStatus.DownloadError, mapPreview.Status);
        }

        [Fact]
        public async Task Install_ShouldSetDownloadErrorStatus_WhenMapInstallPackageIsNull()
        {
            // Arrange
            var mockHttpClient = new Mock<HttpClient>();
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            response.Content = new StringContent("");
            response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment") { FileName = "map.zip" };
            mockHttpClient.Setup(client => client.GetAsync(It.IsAny<string>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(mockHttpClient.Object);

            var mapPreview = new MapPreview(new ModData(), "uid", MapGridType.Rectangular, new MapCache());

            // Act
            await mapPreview.Install("http://example.com");

            // Assert
            Assert.Equal(MapStatus.DownloadError, mapPreview.Status);
        }
    }
}
