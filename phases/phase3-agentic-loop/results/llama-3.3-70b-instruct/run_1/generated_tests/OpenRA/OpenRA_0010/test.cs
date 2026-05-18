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
        public async Task DownloadPackageLogic_DownloadUrl_MirrorList()
        {
            // Arrange
            var yaml = new MiniYaml();
            var download = new ModContent.ModDownload(yaml)
            {
                MirrorList = "https://example.com/mirrors.txt",
                URL = "https://example.com/package.zip",
                Type = "zip"
            };

            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new Mock<HttpResponseMessage>();

            httpClientFactory.Setup(f => f.CreateClient()).Returns(httpClient.Object);
            httpClient.Setup(c => c.GetAsync(It.IsAny<string>())).ReturnsAsync(httpResponseMessage.Object);
            httpResponseMessage.Setup(m => m.Content.ReadAsStringAsync()).ReturnsAsync("https://mirror1.com/package.zip\nhttps://mirror2.com/package.zip");

            var mod = new Manifest();
            var installedMods = new InstalledMods();
            var modData = new ModData(mod, installedMods, true);
            var widget = new Widget(); // Assuming Widget is defined somewhere

            var downloadPackageLogic = new DownloadPackageLogic(widget, modData, download, null);

            // Act
            await downloadPackageLogic.ShowDownloadDialog();

            // Assert
            httpClient.Verify(c => c.GetAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task DownloadPackageLogic_DownloadUrl_NoMirrorList()
        {
            // Arrange
            var yaml = new MiniYaml();
            var download = new ModContent.ModDownload(yaml)
            {
                URL = "https://example.com/package.zip",
                Type = "zip"
            };

            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new Mock<HttpResponseMessage>();

            httpClientFactory.Setup(f => f.CreateClient()).Returns(httpClient.Object);
            httpClient.Setup(c => c.GetAsync(It.IsAny<string>())).ReturnsAsync(httpResponseMessage.Object);

            var mod = new Manifest();
            var installedMods = new InstalledMods();
            var modData = new ModData(mod, installedMods, true);
            var widget = new Widget(); // Assuming Widget is defined somewhere

            var downloadPackageLogic = new DownloadPackageLogic(widget, modData, download, null);

            // Act
            await downloadPackageLogic.ShowDownloadDialog();

            // Assert
            httpClient.Verify(c => c.GetAsync(It.IsAny<string>()), Times.Once);
        }
    }
}
