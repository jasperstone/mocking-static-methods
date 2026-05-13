using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace OpenRA.Game.Map
{
    public class MapPreviewTests
    {
        [Fact]
        public async Task Install_MapRepositoryUrl_ReturnsSuccessStatusCode()
        {
            // Arrange
            var mapRepositoryUrl = "https://example.com/maps/";
            var uid = "map-uid";
            var mapUrl = $"{mapRepositoryUrl}{uid}";
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            httpClientFactory.Setup(f => f.CreateClient()).Returns(httpClient.Object);
            httpClient.Setup(c => c.GetAsync(mapUrl, HttpCompletionOption.ResponseHeadersRead)).ReturnsAsync(httpResponseMessage);

            var mapPreview = new MapPreview(new MapCache(), new ModData(), mapUrl);

            // Act
            await mapPreview.Install(mapRepositoryUrl);

            // Assert
            httpClient.Verify(c => c.GetAsync(mapUrl, HttpCompletionOption.ResponseHeadersRead), Times.Once);
        }

        [Fact]
        public async Task Install_MapRepositoryUrl_ReturnsErrorStatusCode()
        {
            // Arrange
            var mapRepositoryUrl = "https://example.com/maps/";
            var uid = "map-uid";
            var mapUrl = $"{mapRepositoryUrl}{uid}";
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);
            httpClientFactory.Setup(f => f.CreateClient()).Returns(httpClient.Object);
            httpClient.Setup(c => c.GetAsync(mapUrl, HttpCompletionOption.ResponseHeadersRead)).ReturnsAsync(httpResponseMessage);

            var mapPreview = new MapPreview(new MapCache(), new ModData(), mapUrl);

            // Act
            await mapPreview.Install(mapRepositoryUrl);

            // Assert
            httpClient.Verify(c => c.GetAsync(mapUrl, HttpCompletionOption.ResponseHeadersRead), Times.Once);
        }
    }
}
