using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using OpenRA.Mods.Common.Widgets.Logic;
using Xunit;

namespace OpenRA.Mods.Common.Widgets.Logic.Tests
{
    public class DownloadPackageLogicTests
    {
        [Fact]
        public async Task DownloadPackageLogic_DownloadUrl_MirrorListAsync()
        {
            // Arrange
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Test mirror list")
            };

            httpClient.Setup(h => h.GetAsync(It.IsAny<string>())).ReturnsAsync(httpResponseMessage);
            httpClientFactory.Setup(h => h.CreateClient()).Returns(httpClient.Object);

            var downloadPackageLogic = new DownloadPackageLogic(null, null, new ModContent.ModDownload { MirrorList = "https://example.com/mirrors" }, null);

            // Act
            await downloadPackageLogic.DownloadUrl("https://example.com/download");

            // Assert
            httpClient.Verify(h => h.GetAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task DownloadPackageLogic_DownloadUrl_MirrorListFailedAsync()
        {
            // Arrange
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);

            httpClient.Setup(h => h.GetAsync(It.IsAny<string>())).ReturnsAsync(httpResponseMessage);
            httpClientFactory.Setup(h => h.CreateClient()).Returns(httpClient.Object);

            var downloadPackageLogic = new DownloadPackageLogic(null, null, new ModContent.ModDownload { MirrorList = "https://example.com/mirrors" }, null);

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => downloadPackageLogic.DownloadUrl("https://example.com/download"));
        }
    }
}
