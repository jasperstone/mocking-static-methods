using Xunit;
using Moq;
using System.Net.Http;
using System.Threading.Tasks;
using OpenRA.Mods.Common;

namespace OpenRA.Tests
{
    public class WebServicesTests
    {
        [Fact]
        public async Task CheckModVersion_CallsGetAsync()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Setup<Task<HttpResponseMessage>>(
                    h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(new HttpResponseMessage());

            var httpClient = new HttpClient(handlerMock.Object);
            var webServices = new WebServices();

            // Act
            await webServices.CheckModVersion();

            // Assert
            handlerMock.Verify(
                h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task CheckModVersion_Outdated_ReturnsOutdated()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Setup<Task<HttpResponseMessage>>(
                    h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    Content = new StringContent("outdated")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var webServices = new WebServices();

            // Act
            await webServices.CheckModVersion();

            // Assert
            Assert.Equal(ModVersionStatus.Outdated, webServices.ModVersionStatus);
        }

        [Fact]
        public async Task CheckModVersion_Unknown_ReturnsUnknown()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Setup<Task<HttpResponseMessage>>(
                    h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    Content = new StringContent("unknown")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var webServices = new WebServices();

            // Act
            await webServices.CheckModVersion();

            // Assert
            Assert.Equal(ModVersionStatus.Unknown, webServices.ModVersionStatus);
        }

        [Fact]
        public async Task CheckModVersion_Playtest_ReturnsPlaytestAvailable()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Setup<Task<HttpResponseMessage>>(
                    h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    Content = new StringContent("playtest")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var webServices = new WebServices();

            // Act
            await webServices.CheckModVersion();

            // Assert
            Assert.Equal(ModVersionStatus.PlaytestAvailable, webServices.ModVersionStatus);
        }
    }
}
