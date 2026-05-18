using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using OpenRA.Mods.Common.Widgets;
using OpenRA.Support;
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
            var modData = new ModData(new Manifest(), new InstalledMods(), false);
            var download = new ModContent.ModDownload(new MiniYaml());
            var onSuccess = () => { };
            var httpClient = new HttpClient();
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);

            // Act
            var downloadPackageLogic = new DownloadPackageLogic(new Widget(), modData, download, onSuccess);
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(httpResponseMessage);
            var httpClientInstance = new HttpClient(handlerMock.Object);
            var httpClientFactory = new HttpClientFactory();
            var originalCreate = httpClientFactory.Create;
            httpClientFactory.Create = () => httpClientInstance;

            // Act
            await downloadPackageLogic.DownloadUrl(download.URL);

            // Assert
            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Exactly(2),
               ItExpr.IsAny<HttpRequestMessage>(),
               ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task DownloadPackageLogic_DownloadUrl_GetAsyncFailed()
        {
            // Arrange
            var modData = new ModData(new Manifest(), new InstalledMods(), false);
            var download = new ModContent.ModDownload(new MiniYaml());
            var onSuccess = () => { };
            var httpClient = new HttpClient();
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);

            // Act
            var downloadPackageLogic = new DownloadPackageLogic(new Widget(), modData, download, onSuccess);
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(httpResponseMessage);
            var httpClientInstance = new HttpClient(handlerMock.Object);
            var httpClientFactory = new HttpClientFactory();
            var originalCreate = httpClientFactory.Create;
            httpClientFactory.Create = () => httpClientInstance;

            // Act and Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => downloadPackageLogic.DownloadUrl(download.URL));
        }
    }
}
