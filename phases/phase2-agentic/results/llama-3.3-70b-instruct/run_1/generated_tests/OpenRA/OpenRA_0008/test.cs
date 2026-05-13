using Moq;
using OpenRA;
using OpenRA.Mods.Common;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace OpenRA.Tests
{
    public class WebServicesTests
    {
        [Fact]
        public async Task CheckModVersion_LatestVersion_ReturnsLatestStatus()
        {
            // Arrange
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("latest") };
            httpClient.Setup(h => h.GetAsync(It.IsAny<string>())).ReturnsAsync(httpResponseMessage);
            httpClientFactory.Setup(h => h.CreateClient()).Returns(httpClient.Object);

            var webServices = new WebServices();
            webServices.ModVersionStatus = ModVersionStatus.NotChecked;

            // Act
            await webServices.CheckModVersion();

            // Assert
            Assert.Equal(ModVersionStatus.Latest, webServices.ModVersionStatus);
        }

        [Fact]
        public async Task CheckModVersion_OutdatedVersion_ReturnsOutdatedStatus()
        {
            // Arrange
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("outdated") };
            httpClient.Setup(h => h.GetAsync(It.IsAny<string>())).ReturnsAsync(httpResponseMessage);
            httpClientFactory.Setup(h => h.CreateClient()).Returns(httpClient.Object);

            var webServices = new WebServices();
            webServices.ModVersionStatus = ModVersionStatus.NotChecked;

            // Act
            await webServices.CheckModVersion();

            // Assert
            Assert.Equal(ModVersionStatus.Outdated, webServices.ModVersionStatus);
        }

        [Fact]
        public async Task CheckModVersion_UnknownVersion_ReturnsUnknownStatus()
        {
            // Arrange
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("unknown") };
            httpClient.Setup(h => h.GetAsync(It.IsAny<string>())).ReturnsAsync(httpResponseMessage);
            httpClientFactory.Setup(h => h.CreateClient()).Returns(httpClient.Object);

            var webServices = new WebServices();
            webServices.ModVersionStatus = ModVersionStatus.NotChecked;

            // Act
            await webServices.CheckModVersion();

            // Assert
            Assert.Equal(ModVersionStatus.Unknown, webServices.ModVersionStatus);
        }

        [Fact]
        public async Task CheckModVersion_PlaytestVersion_ReturnsPlaytestAvailableStatus()
        {
            // Arrange
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("playtest") };
            httpClient.Setup(h => h.GetAsync(It.IsAny<string>())).ReturnsAsync(httpResponseMessage);
            httpClientFactory.Setup(h => h.CreateClient()).Returns(httpClient.Object);

            var webServices = new WebServices();
            webServices.ModVersionStatus = ModVersionStatus.NotChecked;

            // Act
            await webServices.CheckModVersion();

            // Assert
            Assert.Equal(ModVersionStatus.PlaytestAvailable, webServices.ModVersionStatus);
        }
    }
}
