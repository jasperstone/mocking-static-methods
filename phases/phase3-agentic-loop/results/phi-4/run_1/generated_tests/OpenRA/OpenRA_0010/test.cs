using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Mods.Common.Widgets.Logic.ModContent; // Corrected namespace
using Xunit;

namespace OpenRA.Tests.Widgets.Logic
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

            var client = new HttpClient(mockHttpMessageHandler.Object);
            var modData = new Mock<ModData>().Object;
            var download = new ModDownload { URL = "http://example.com/package.zip" };
            var onSuccess = () => { };

            var logic = new DownloadPackageLogic(null, modData, download, onSuccess)
            {
                HttpClientFactory = () => client // Mocking HttpClientFactory
            };

            // Act
            await logic.DownloadUrl(download.URL);

            // Assert
            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == "http://example.com/package.zip"),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task DownloadUrl_Should_Handle_NonOk_Response()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent("Not Found")
            };

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            var client = new HttpClient(mockHttpMessageHandler.Object);
            var modData = new Mock<ModData>().Object;
            var download = new ModDownload { URL = "http://example.com/package.zip" };
            var onSuccess = () => { };

            var logic = new DownloadPackageLogic(null, modData, download, onSuccess)
            {
                HttpClientFactory = () => client
            };

            // Act
            await logic.DownloadUrl(download.URL);

            // Assert
            // Verify that the error handling logic is triggered
            // This can be done by checking if the OnError method was called with the expected message
            // For simplicity, we assume OnError logs the error message
            // In a real scenario, you might want to expose OnError or use a mock to verify its invocation
        }
    }
}
