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
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent("latest")
                });
            var httpClient = new HttpClient(handlerMock.Object);

            var webServices = new WebServices();
            webServices.GetType().GetProperty("ModVersionStatus").SetValue(webServices, ModVersionStatus.NotChecked);

            // Act
            webServices.CheckModVersion();
            await Task.Delay(100); // Wait for the task to complete

            // Assert
            Assert.Equal(ModVersionStatus.Latest, webServices.ModVersionStatus);
            handlerMock.Verify(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CheckModVersion_OutdatedStatus()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent("outdated")
                });
            var httpClient = new HttpClient(handlerMock.Object);

            var webServices = new WebServices();
            webServices.GetType().GetProperty("ModVersionStatus").SetValue(webServices, ModVersionStatus.NotChecked);

            // Act
            webServices.CheckModVersion();
            await Task.Delay(100); // Wait for the task to complete

            // Assert
            Assert.Equal(ModVersionStatus.Outdated, webServices.ModVersionStatus);
            handlerMock.Verify(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CheckModVersion_UnknownStatus()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent("unknown")
                });
            var httpClient = new HttpClient(handlerMock.Object);

            var webServices = new WebServices();
            webServices.GetType().GetProperty("ModVersionStatus").SetValue(webServices, ModVersionStatus.NotChecked);

            // Act
            webServices.CheckModVersion();
            await Task.Delay(100); // Wait for the task to complete

            // Assert
            Assert.Equal(ModVersionStatus.Unknown, webServices.ModVersionStatus);
            handlerMock.Verify(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CheckModVersion_PlaytestAvailableStatus()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent("playtest")
                });
            var httpClient = new HttpClient(handlerMock.Object);

            var webServices = new WebServices();
            webServices.GetType().GetProperty("ModVersionStatus").SetValue(webServices, ModVersionStatus.NotChecked);

            // Act
            webServices.CheckModVersion();
            await Task.Delay(100); // Wait for the task to complete

            // Assert
            Assert.Equal(ModVersionStatus.PlaytestAvailable, webServices.ModVersionStatus);
            handlerMock.Verify(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
