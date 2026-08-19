using System;
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
        public async Task DownloadUrl_MirrorList_DownloadsMirrorList()
        {
            // Arrange
            var modData = new ModData(new Manifest("test", new Package("test")), new InstalledMods(new[] { "test" }, new[] { "test" }), false);
            var download = new ModContent.ModDownload(new MiniYaml());
            download.Title = "Test Download";
            download.URL = "https://example.com/download";
            download.MirrorList = "https://example.com/mirrors";
            var onSuccess = () => { };
            var downloadPackageLogic = new DownloadPackageLogic(new Widget(), modData, download, onSuccess);

            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("Mirror1\nMirror2")
            };
            httpClient.Setup(c => c.GetAsync(It.IsAny<string>())).ReturnsAsync(httpResponseMessage);
            httpClientFactory.Setup(f => f.CreateClient()).Returns(httpClient.Object);

            // Act
            await downloadPackageLogic.DownloadUrl(download.MirrorList);

            // Assert
            httpClient.Verify(c => c.GetAsync(It.IsAny<string>()), Times.Once);
        }
    }
}
