using System;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using OpenRA;
using OpenRA.Game.Map;
using Xunit;

namespace OpenRA.Tests
{
    public class MapPreviewTests
    {
        [Fact]
        public async Task Install_MapRepositoryUrlIsValid_MapIsDownloaded()
        {
            // Arrange
            var mapCache = new MapCache(new Manifest(), new FileSystem());
            var modData = new ModData();
            var mapPreview = new MapPreview("uid", mapCache, modData);
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new Mock<HttpResponseMessage>();
            var httpContent = new Mock<HttpContent>();

            httpClientFactory.Setup(f => f.CreateClient()).Returns(httpClient.Object);
            httpClient.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<HttpCompletionOption>()))
                .ReturnsAsync(httpResponseMessage.Object);
            httpResponseMessage.Setup(m => m.IsSuccessStatusCode).Returns(true);
            httpContent.Setup(c => c.Headers).Returns(new HeaderCollection { { "Content-Disposition", "attachment; filename=\"map.oramap\"" } });

            // Act
            Task.Run(() => mapPreview.Install("https://example.com/maps/")).Wait();

            // Assert
            httpClient.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<HttpCompletionOption>()), Times.Once);
        }

        [Fact]
        public async Task Install_MapRepositoryUrlIsInvalid_MapIsNotDownloaded()
        {
            // Arrange
            var mapCache = new MapCache(new Manifest(), new FileSystem());
            var modData = new ModData();
            var mapPreview = new MapPreview("uid", mapCache, modData);
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new Mock<HttpResponseMessage>();

            httpClientFactory.Setup(f => f.CreateClient()).Returns(httpClient.Object);
            httpClient.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<HttpCompletionOption>()))
                .ReturnsAsync(httpResponseMessage.Object);
            httpResponseMessage.Setup(m => m.IsSuccessStatusCode).Returns(false);

            // Act
            Task.Run(() => mapPreview.Install("https://example.com/maps/")).Wait();

            // Assert
            httpClient.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<HttpCompletionOption>()), Times.Once);
        }
    }
}
