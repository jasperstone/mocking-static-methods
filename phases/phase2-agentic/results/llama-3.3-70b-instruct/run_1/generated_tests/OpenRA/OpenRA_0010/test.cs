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
        public async Task DownloadPackageLogic_DownloadUrl_GetAsyncCalled()
        {
            // Arrange
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var httpClientMock = new Mock<HttpClient>();
            var httpClientHandlerMock = new Mock<HttpMessageHandler>();

            httpClientHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage());

            httpClientMock
                .Setup(h => h.GetAsync(It.IsAny<string>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage());

            var downloadPackageLogic = new DownloadPackageLogic(
                new Widget(),
                new ModData(),
                new ModContent.ModDownload(),
                () => { }
            );

            // Act
            await downloadPackageLogic.DownloadUrl("https://example.com");

            // Assert
            httpClientMock.Verify(h => h.GetAsync(It.IsAny<string>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
