using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Widgets;
using Xunit;

namespace OpenRA.Mods.Common.Tests.Widgets.Logic.Installation
{
    public class DownloadPackageLogicTests
    {
        [Fact]
        public async Task DownloadUrl_ShouldCallGetAsync_WhenMirrorListIsNotNull()
        {
            // Arrange
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);
            mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(mockHttpClient);

            var download = new ModContent.ModDownload
            {
                MirrorList = "http://example.com/mirrors"
            };

            var logic = new DownloadPackageLogic(new Widget(), new ModData(), download, () => { });

            // Act
            logic.DownloadUrl(download.MirrorList);

            // Assert
            mockHttpClientFactory.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Once);
            mockHttpMessageHandler.Verify(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
