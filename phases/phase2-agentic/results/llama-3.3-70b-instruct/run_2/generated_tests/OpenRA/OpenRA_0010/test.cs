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
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var httpClientMock = new Mock<HttpClient>();
            var httpResponseMessageMock = new Mock<HttpResponseMessage>();

            httpClientFactoryMock.Setup(x => x.CreateClient()).Returns(httpClientMock.Object);
            httpClientMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(httpResponseMessageMock.Object);
            httpResponseMessageMock.Setup(x => x.Content.ReadAsStringAsync()).ReturnsAsync("https://example.com/mirror1\nhttps://example.com/mirror2");

            var download = new ModContent.ModDownload
            {
                MirrorList = "https://example.com/mirrors"
            };

            var downloadPackageLogic = new DownloadPackageLogic(null, null, download, null);

            // Act
            await downloadPackageLogic.DownloadUrl(download.MirrorList);

            // Assert
            httpClientMock.Verify(x => x.GetAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task DownloadPackageLogic_DownloadUrl_NoMirrorList()
        {
            // Arrange
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var httpClientMock = new Mock<HttpClient>();
            var httpResponseMessageMock = new Mock<HttpResponseMessage>();

            httpClientFactoryMock.Setup(x => x.CreateClient()).Returns(httpClientMock.Object);
            httpClientMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(httpResponseMessageMock.Object);
            httpResponseMessageMock.Setup(x => x.Content.ReadAsStringAsync()).ReturnsAsync("");

            var download = new ModContent.ModDownload
            {
                URL = "https://example.com/package"
            };

            var downloadPackageLogic = new DownloadPackageLogic(null, null, download, null);

            // Act
            await downloadPackageLogic.DownloadUrl(download.URL);

            // Assert
            httpClientMock.Verify(x => x.GetAsync(It.IsAny<string>()), Times.Once);
        }
    }
}
