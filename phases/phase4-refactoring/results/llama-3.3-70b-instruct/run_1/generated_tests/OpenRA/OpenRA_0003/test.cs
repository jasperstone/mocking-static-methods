using Xunit;
using Moq;
using System.Net.Http;
using System.Threading.Tasks;

namespace OpenRA.Game.Map.Tests
{
    public class MapPreviewTests
    {
        [Fact]
        public async Task Install_DownloadSuccess_MapInstalled()
        {
            // Arrange
            var mapPreview = new MapPreview(new ModData(), "map", MapGridType.Default, new MapCache());
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new Mock<HttpResponseMessage>();
            var httpContent = new Mock<HttpContent>();

            httpClientFactory.Setup(f => f.CreateClient()).Returns(httpClient.Object);
            httpClient.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<HttpCompletionOption>()))
                .ReturnsAsync(httpResponseMessage.Object);
            httpResponseMessage.Setup(m => m.IsSuccessStatusCode).Returns(true);
            httpResponseMessage.Setup(m => m.Content).Returns(httpContent.Object);
            httpContent.Setup(c => c.Headers).Returns(new HttpHeaders { ContentDisposition = "attachment; filename=map.oramap" });

            // Act
            await mapPreview.Install("https://example.com/maps/", httpClientFactory.Object);

            // Assert
            Assert.Equal(MapStatus.Installed, mapPreview.Status);
        }

        [Fact]
        public async Task Install_DownloadFailure_MapNotInstalled()
        {
            // Arrange
            var mapPreview = new MapPreview(new ModData(), "map", MapGridType.Default, new MapCache());
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new Mock<HttpResponseMessage>();

            httpClientFactory.Setup(f => f.CreateClient()).Returns(httpClient.Object);
            httpClient.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<HttpCompletionOption>()))
                .ReturnsAsync(httpResponseMessage.Object);
            httpResponseMessage.Setup(m => m.IsSuccessStatusCode).Returns(false);

            // Act
            await mapPreview.Install("https://example.com/maps/", httpClientFactory.Object);

            // Assert
            Assert.Equal(MapStatus.DownloadError, mapPreview.Status);
        }
    }
}
