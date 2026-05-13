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
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("mock content")
            };

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var download = new ModContent.ModDownload
            {
                MirrorList = "http://example.com/mirrorlist",
                URL = "http://example.com/default"
            };

            var logic = new DownloadPackageLogic(null, null, download, () => { });

            // Act
            await logic.DownloadUrl(download.MirrorList);

            // Assert
            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString() == download.MirrorList),
                ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}
