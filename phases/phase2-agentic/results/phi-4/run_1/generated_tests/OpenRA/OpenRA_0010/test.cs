using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA.Mods.Common.Widgets.Logic;
using Xunit;

namespace OpenRA.Tests
{
    public class DownloadPackageLogicTests
    {
        [Fact]
        public async Task DownloadUrl_Should_Call_GetAsync_With_Correct_Url()
        {
            // Arrange
            var modData = new ModData();
            var download = new ModContent.ModDownload
            {
                MirrorList = "http://example.com/mirrorlist",
                URL = "http://example.com/package"
            };
            var onSuccess = () => { };

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("mirror1\nmirror2")
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            HttpClientFactory.Create = () => client;

            var logic = new DownloadPackageLogic(null, modData, download, onSuccess);

            // Act
            logic.ShowDownloadDialog();

            // Assert
            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == "http://example.com/mirrorlist"),
                ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}
