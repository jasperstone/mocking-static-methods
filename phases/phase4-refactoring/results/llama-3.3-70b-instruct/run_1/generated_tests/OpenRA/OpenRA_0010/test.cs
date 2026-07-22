using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Widgets;
using Xunit;

namespace OpenRA.Mods.Common.Tests
{
    public class DownloadPackageLogicTests
    {
        [Fact]
        public async Task DownloadPackageLogic_DownloadUrl_GetAsyncCalled()
        {
            // Arrange
            var modData = new ModData(new Manifest("test"), new InstalledMods(new[] { "path1" }, new[] { "path2" }), false);
            var download = new ModContent.ModDownload(new MiniYaml("Test Download"));
            download.URL = "https://example.com/download";
            var onSuccess = new Action(() => { });
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            httpClient.Setup(h => h.GetAsync(It.IsAny<string>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(httpResponseMessage);
            httpClientFactory.Setup(h => h.CreateClient()).Returns(httpClient.Object);

            // Act
            var downloadPackageLogic = new DownloadPackageLogic(new Widget(), modData, download, onSuccess);
            await downloadPackageLogic.DownloadUrl(download.URL);

            // Assert
            httpClient.Verify(h => h.GetAsync(It.IsAny<string>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DownloadPackageLogic_DownloadUrl_GetAsyncFailed()
        {
            // Arrange
            var modData = new ModData(new Manifest("test"), new InstalledMods(new[] { "path1" }, new[] { "path2" }), false);
            var download = new ModContent.ModDownload(new MiniYaml("Test Download"));
            download.URL = "https://example.com/download";
            var onSuccess = new Action(() => { });
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);
            httpClient.Setup(h => h.GetAsync(It.IsAny<string>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(httpResponseMessage);
            httpClientFactory.Setup(h => h.CreateClient()).Returns(httpClient.Object);

            // Act
            var downloadPackageLogic = new DownloadPackageLogic(new Widget(), modData, download, onSuccess);
            await downloadPackageLogic.DownloadUrl(download.URL);

            // Assert
            httpClient.Verify(h => h.GetAsync(It.IsAny<string>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
