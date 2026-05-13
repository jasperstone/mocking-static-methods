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

            var mockHttpClient = new Mock<HttpClient>();
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("mapFilename")
            };

            mockHttpClient
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(responseMessage);

            var clientFactory = new Mock<IHttpClientFactory>();
            clientFactory
                .Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(mockHttpClient.Object);

            // Act
            await mapPreview.Install("http://example.com/maps/", clientFactory.Object);

            // Assert
            Assert.Equal(MapStatus.DownloadError, mapPreview.innerData.Status);
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

            var mockHttpClient = new Mock<HttpClient>();
            var clientFactory = new Mock<IHttpClientFactory>();
            clientFactory
                .Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(mockHttpClient.Object);

            // Act
            await mapPreview.Install("http://example.com/maps/", clientFactory.Object);

            // Assert
            Assert.Equal(MapStatus.Available, mapPreview.innerData.Status);
        }

        [Fact]
        public async Task Install_ShouldHandleDownloadFailure()
        {
            // Arrange
            var mapPreview = new MapPreview
            {
                innerData = new MapPreview.InnerData
                {
                    Status = MapStatus.DownloadAvailable
                }
            };

            var mockHttpClient = new Mock<HttpClient>();
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent("Not Found")
            };

            mockHttpClient
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(responseMessage);

            var clientFactory = new Mock<IHttpClientFactory>();
            clientFactory
                .Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(mockHttpClient.Object);

            // Act
            await mapPreview.Install("http://example.com/maps/", clientFactory.Object);

            // Assert
            Assert.Equal(MapStatus.DownloadError, mapPreview.innerData.Status);
        }
    }
}
