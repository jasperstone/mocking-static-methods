using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using OpenRA.Game.Map;

namespace OpenRA.Tests
{
    public class MapPreviewTests
    {
        [Fact]
        public async Task Install_Should_Handle_NonSuccessStatusCode()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new HttpResponseMessage
               {
                   StatusCode = HttpStatusCode.NotFound,
                   Content = new StringContent("Not Found")
               });

            var httpClient = new HttpClient(handlerMock.Object);

            // Assume MapPreview has a constructor that accepts HttpClient
            var mapPreview = new MapPreview(/* dependencies */, httpClient);

            // Act
            await mapPreview.Install("http://example.com/maps/");

            // Assert
            Assert.Equal(MapStatus.DownloadError, mapPreview.Status);
        }
    }
}
