using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using OpenRA.Mods.Common;
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
            var httpMessageHandler = new Mock<HttpMessageHandler>();

            httpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("latest")
                });

            httpClient
                .Setup(h => h.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("latest")
                });

            httpClientFactory
                .Setup(h => h.CreateClient(It.IsAny<string>()))
                .Returns(httpClient.Object);

            var webServices = new WebServices();

            // Act
            webServices.CheckModVersion();

            // Assert
            await Task.Delay(100); // wait for the task to complete
            Assert.Equal(ModVersionStatus.Latest, webServices.ModVersionStatus);
        }

        [Fact]
        public async Task CheckModVersion_OutdatedVersion_ReturnsOutdatedStatus()
        {
            // Arrange
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var httpMessageHandler = new Mock<HttpMessageHandler>();

            httpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("outdated")
                });

            httpClient
                .Setup(h => h.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("outdated")
                });

            httpClientFactory
                .Setup(h => h.CreateClient(It.IsAny<string>()))
                .Returns(httpClient.Object);

            var webServices = new WebServices();

            // Act
            webServices.CheckModVersion();

            // Assert
            await Task.Delay(100); // wait for the task to complete
            Assert.Equal(ModVersionStatus.Outdated, webServices.ModVersionStatus);
        }

        [Fact]
        public async Task CheckModVersion_UnknownVersion_ReturnsUnknownStatus()
        {
            // Arrange
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var httpMessageHandler = new Mock<HttpMessageHandler>();

            httpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("unknown")
                });

            httpClient
                .Setup(h => h.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("unknown")
                });

            httpClientFactory
                .Setup(h => h.CreateClient(It.IsAny<string>()))
                .Returns(httpClient.Object);

            var webServices = new WebServices();

            // Act
            webServices.CheckModVersion();

            // Assert
            await Task.Delay(100); // wait for the task to complete
            Assert.Equal(ModVersionStatus.Unknown, webServices.ModVersionStatus);
        }

        [Fact]
        public async Task CheckModVersion_PlaytestVersion_ReturnsPlaytestAvailableStatus()
        {
            // Arrange
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var httpMessageHandler = new Mock<HttpMessageHandler>();

            httpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("playtest")
                });

            httpClient
                .Setup(h => h.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("playtest")
                });

            httpClientFactory
                .Setup(h => h.CreateClient(It.IsAny<string>()))
                .Returns(httpClient.Object);

            var webServices = new WebServices();

            // Act
            webServices.CheckModVersion();

            // Assert
            await Task.Delay(100); // wait for the task to complete
            Assert.Equal(ModVersionStatus.PlaytestAvailable, webServices.ModVersionStatus);
        }
    }
}
