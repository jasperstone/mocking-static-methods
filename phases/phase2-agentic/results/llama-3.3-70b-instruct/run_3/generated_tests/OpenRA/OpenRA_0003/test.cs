using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using OpenRA.Game.Map;
using Xunit;

namespace OpenRA.Game.Tests
{
    public class MapPreviewTests
    {
        [Fact]
        public async Task Install_MapRepositoryUrl_ReturnsSuccess()
        {
            // Arrange
            var mapRepositoryUrl = "https://example.com/maps/";
            var uid = "map-uid";
            var mapPreview = new MapPreview("map-uid", null, null);
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new Mock<HttpResponseMessage>();

            httpClientFactory.Setup(f => f.CreateClient()).Returns(httpClient.Object);
            httpClient.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<HttpCompletionOption>()))
                .ReturnsAsync(httpResponseMessage.Object);
            httpResponseMessage.SetupGet(r => r.IsSuccessStatusCode).Returns(true);

            // Act
            await mapPreview.Install(mapRepositoryUrl);

            // Assert
            httpClient.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<HttpCompletionOption>()), Times.Once);
        }

        [Fact]
        public async Task Install_MapRepositoryUrl_ReturnsFailure()
        {
            // Arrange
            var mapRepositoryUrl = "https://example.com/maps/";
            var uid = "map-uid";
            var mapPreview = new MapPreview("map-uid", null, null);
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new Mock<HttpResponseMessage>();

            httpClientFactory.Setup(f => f.CreateClient()).Returns(httpClient.Object);
            httpClient.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<HttpCompletionOption>()))
                .ReturnsAsync(httpResponseMessage.Object);
            httpResponseMessage.SetupGet(r => r.IsSuccessStatusCode).Returns(false);

            // Act
            await mapPreview.Install(mapRepositoryUrl);

            // Assert
            httpClient.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<HttpCompletionOption>()), Times.Once);
        }
    }
}
