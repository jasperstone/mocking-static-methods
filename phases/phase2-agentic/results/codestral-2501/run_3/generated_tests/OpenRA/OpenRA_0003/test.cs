using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using OpenRA;
using OpenRA.FileSystem;
using OpenRA.Graphics;
using OpenRA.Primitives;
using Xunit;

namespace OpenRA.Tests
{
    public class MapPreviewTests
    {
        [Fact]
        public async Task Install_ShouldDownloadMapSuccessfully()
        {
            // Arrange
            var mapRepositoryUrl = "http://example.com/maps/";
            var mapUid = "test-map";
            var mapFilename = "test-map.oramap";
            var mapContent = new byte[] { 0x01, 0x02, 0x03, 0x04 }; // Dummy map content

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new ByteArrayContent(mapContent)
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var mockMapInstallPackage = new Mock<IReadWritePackage>();
            mockMapInstallPackage.Setup(p => p.Update(It.IsAny<string>(), It.IsAny<byte[]>())).Verifiable();

            var mapCache = new MapCache(new Manifest(), new FileSystem());
            var modData = new ModData(new Manifest(), new FileSystem());
            var mapPreview = new MapPreview(modData, mapUid, MapGridType.Rectangular, mapCache);

            // Act
            mapPreview.Install(mapRepositoryUrl);

            // Assert
            await Task.Delay(1000); // Wait for the async operation to complete
            mockMapInstallPackage.Verify(p => p.Update(mapFilename, mapContent), Times.Once);
        }

        [Fact]
        public async Task Install_ShouldHandleDownloadError()
        {
            // Arrange
            var mapRepositoryUrl = "http://example.com/maps/";
            var mapUid = "test-map";

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var mapCache = new MapCache(new Manifest(), new FileSystem());
            var modData = new ModData(new Manifest(), new FileSystem());
            var mapPreview = new MapPreview(modData, mapUid, MapGridType.Rectangular, mapCache);

            // Act
            mapPreview.Install(mapRepositoryUrl);

            // Assert
            await Task.Delay(1000); // Wait for the async operation to complete
            Assert.Equal(MapStatus.DownloadError, mapPreview.Status);
        }
    }
}
