using Moq;
using Moq.Protected;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OpenRA.Game.Map.Tests
{
    public class MapPreviewTests
    {
        [Fact]
        public async Task Install_ShouldDownloadMap_WhenStatusIsDownloadAvailable()
        {
            // Arrange
            var mapPreview = new MapPreview
            {
                innerData = new MapPreview.InnerData
                {
                    Status = MapStatus.DownloadAvailable
                }
            };

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("mock content")
                })
                .Verifiable();

            var client = new HttpClient(mockHttpMessageHandler.Object);

            // Act
            await mapPreview.Install("http://example.com/maps/");

            // Assert
            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("http://example.com/maps/")),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task Install_ShouldNotDownloadMap_WhenStatusIsNotDownloadAvailable()
        {
            // Arrange
            var mapPreview = new MapPreview
            {
                innerData = new MapPreview.InnerData
                {
                    Status = MapStatus.Available
                }
            };

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var client = new HttpClient(mockHttpMessageHandler.Object);

            // Act
            await mapPreview.Install("http://example.com/maps/");

            // Assert
            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task Install_ShouldNotDownloadMap_WhenDownloadingIsNotAllowed()
        {
            // Arrange
            var mapPreview = new MapPreview
            {
                innerData = new MapPreview.InnerData
                {
                    Status = MapStatus.DownloadAvailable
                }
            };

            // Simulate game settings not allowing downloading
            mapPreview.Game = new Game
            {
                Settings = new GameSettings
                {
                    Game = new GameSettings.GameSettings
                    {
                        AllowDownloading = false
                    }
                }
            };

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var client = new HttpClient(mockHttpMessageHandler.Object);

            // Act
            await mapPreview.Install("http://example.com/maps/");

            // Assert
            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task Install_ShouldHandleExceptionDuringDownload()
        {
            // Arrange
            var mapPreview = new MapPreview
            {
                innerData = new MapPreview.InnerData
                {
                    Status = MapStatus.DownloadAvailable
                }
            };

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Network error"))
                .Verifiable();

            var client = new HttpClient(mockHttpMessageHandler.Object);

            // Act
            await mapPreview.Install("http://example.com/maps/");

            // Assert
            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );

            Assert.Equal(MapStatus.DownloadError, mapPreview.innerData.Status);
        }
    }
}
