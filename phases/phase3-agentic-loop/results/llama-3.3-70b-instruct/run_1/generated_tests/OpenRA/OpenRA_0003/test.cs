using Moq;
using OpenRA;
using OpenRA.Game;
using OpenRA.Mods.Common;
using OpenRA.Traits;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OpenRA.Tests
{
    public class MapPreviewTests
    {
        [Fact]
        public async Task Install_MapRepositoryUrlIsValid_MapIsDownloaded()
        {
            // Arrange
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new Mock<HttpResponseMessage>();
            var httpContent = new Mock<HttpContent>();

            httpClientFactory.Setup(f => f.CreateClient()).Returns(httpClient.Object);
            httpClient.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<HttpCompletionOption>())).ReturnsAsync(httpResponseMessage.Object);
            httpResponseMessage.Setup(m => m.IsSuccessStatusCode).Returns(true);
            httpContent.Setup(c => c.ReadAsStreamAsync()).ReturnsAsync(new MemoryStream());

            var mapPreview = new MapPreview(new MapCache(new Manifest(), new FileSystem()), new OpenRA.Mods.Common.ModData(), "uid");

            // Act
            mapPreview.Install("https://example.com/maps");

            // Assert
            httpClient.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<HttpCompletionOption>()), Times.Once);
        }

        [Fact]
        public void Install_MapRepositoryUrlIsInvalid_MapIsNotDownloaded()
        {
            // Arrange
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new Mock<HttpResponseMessage>();

            httpClientFactory.Setup(f => f.CreateClient()).Returns(httpClient.Object);
            httpClient.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<HttpCompletionOption>())).ReturnsAsync(httpResponseMessage.Object);
            httpResponseMessage.Setup(m => m.IsSuccessStatusCode).Returns(false);

            var mapPreview = new MapPreview(new MapCache(new Manifest(), new FileSystem()), new OpenRA.Mods.Common.ModData(), "uid");

            // Act
            mapPreview.Install("https://example.com/maps");

            // Assert
            httpClient.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<HttpCompletionOption>()), Times.Once);
        }
    }
}
