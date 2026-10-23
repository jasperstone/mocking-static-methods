using Xunit;
using Moq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;

namespace OpenRA.Game.Map.Tests
{
    public class MapPreviewTests
    {
        [Fact]
        public async Task Install_DownloadSuccess_MapInstalled()
        {
            // Arrange
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new Mock<HttpResponseMessage>();
            var httpContent = new Mock<HttpContent>();

            httpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient.Object);
            httpClient.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<HttpCompletionOption>())).ReturnsAsync(httpResponseMessage.Object);
            httpResponseMessage.Setup(m => m.IsSuccessStatusCode).Returns(true);
            httpResponseMessage.Setup(m => m.Content).Returns(httpContent.Object);
            httpContent.Setup(c => c.Headers).Returns(new HttpResponseHeaders { ContentDisposition = "attachment; filename=map.oramap" });

            var mapPreview = new MapPreview(new ModData(new Manifest("modId", new ReadOnlyPackage("package")), new InstalledMods(new[] { "path1", "path2" }, new[] { "path3", "path4" }), false), "", MapGridType.Default, new MapCache(new Manifest("modId", new ReadOnlyPackage("package")), new FileSystem()));

            // Act
            await mapPreview.Install("https://example.com/maps/");

            // Assert
            httpClient.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<HttpCompletionOption>()), Times.Once);
        }

        [Fact]
        public async Task Install_DownloadFailure_MapNotInstalled()
        {
            // Arrange
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new Mock<HttpResponseMessage>();

            httpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient.Object);
            httpClient.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<HttpCompletionOption>())).ReturnsAsync(httpResponseMessage.Object);
            httpResponseMessage.Setup(m => m.IsSuccessStatusCode).Returns(false);

            var mapPreview = new MapPreview(new ModData(new Manifest("modId", new ReadOnlyPackage("package")), new InstalledMods(new[] { "path1", "path2" }, new[] { "path3", "path4" }), false), "", MapGridType.Default, new MapCache(new Manifest("modId", new ReadOnlyPackage("package")), new FileSystem()));

            // Act
            await mapPreview.Install("https://example.com/maps/");

            // Assert
            httpClient.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<HttpCompletionOption>()), Times.Once);
        }
    }
}
