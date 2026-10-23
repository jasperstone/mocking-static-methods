using Xunit;
using Moq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;

namespace OpenRA.Mods.Common.Tests
{
    public class WebServicesTests
    {
        [Fact]
        public async Task CheckModVersion_LatestVersion_ReturnsLatestStatus()
        {
            // Arrange
            var webServices = new WebServices();
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("latest") });
            var httpClient = new HttpClient(handlerMock.Object);

            // Act
            await webServices.CheckModVersion();

            // Assert
            Assert.Equal(ModVersionStatus.Latest, webServices.ModVersionStatus);
        }

        [Fact]
        public async Task CheckModVersion_OutdatedVersion_ReturnsOutdatedStatus()
        {
            // Arrange
            var webServices = new WebServices();
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("outdated") });
            var httpClient = new HttpClient(handlerMock.Object);

            // Act
            await webServices.CheckModVersion();

            // Assert
            Assert.Equal(ModVersionStatus.Outdated, webServices.ModVersionStatus);
        }

        [Fact]
        public async Task CheckModVersion_UnknownVersion_ReturnsUnknownStatus()
        {
            // Arrange
            var webServices = new WebServices();
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("unknown") });
            var httpClient = new HttpClient(handlerMock.Object);

            // Act
            await webServices.CheckModVersion();

            // Assert
            Assert.Equal(ModVersionStatus.Unknown, webServices.ModVersionStatus);
        }

        [Fact]
        public async Task CheckModVersion_PlaytestVersion_ReturnsPlaytestAvailableStatus()
        {
            // Arrange
            var webServices = new WebServices();
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("playtest") });
            var httpClient = new HttpClient(handlerMock.Object);

            // Act
            await webServices.CheckModVersion();

            // Assert
            Assert.Equal(ModVersionStatus.PlaytestAvailable, webServices.ModVersionStatus);
        }
    }
}
