using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Support;
using Xunit;

namespace OpenRA.Mods.Common.Tests.Widgets.Logic.Installation
{
    public class DownloadPackageLogicTests
    {
        [Fact]
        public async Task DownloadUrl_ShouldCallGetAsync_WhenUrlIsValid()
        {
            // Arrange
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var mockHttpClient = new Mock<HttpClient>();
            var mockHttpResponseMessage = new Mock<HttpResponseMessage>();
            var mockHttpContent = new Mock<HttpContent>();

            mockHttpClientFactory.Setup(f => f.Create()).Returns(mockHttpClient.Object);
            mockHttpResponseMessage.Setup(r => r.StatusCode).Returns(HttpStatusCode.OK);
            mockHttpResponseMessage.Setup(r => r.Content).Returns(mockHttpContent.Object);
            mockHttpClient.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockHttpResponseMessage.Object);

            var downloadPackageLogic = new DownloadPackageLogic(
                new Widget(),
                new ModData(),
                new ModContent.ModDownload { URL = "http://example.com" },
                () => { });

            // Act
            await downloadPackageLogic.DownloadUrl("http://example.com");

            // Assert
            mockHttpClient.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DownloadUrl_ShouldLogError_WhenHttpStatusCodeIsNotOk()
        {
            // Arrange
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var mockHttpClient = new Mock<HttpClient>();
            var mockHttpResponseMessage = new Mock<HttpResponseMessage>();

            mockHttpClientFactory.Setup(f => f.Create()).Returns(mockHttpClient.Object);
            mockHttpResponseMessage.Setup(r => r.StatusCode).Returns(HttpStatusCode.NotFound);
            mockHttpClient.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockHttpResponseMessage.Object);

            var downloadPackageLogic = new DownloadPackageLogic(
                new Widget(),
                new ModData(),
                new ModContent.ModDownload { URL = "http://example.com" },
                () => { });

            // Act
            await downloadPackageLogic.DownloadUrl("http://example.com");

            // Assert
            mockHttpClient.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()), Times.Once);
            // Add assertions to verify logging or error handling
        }
    }
}
