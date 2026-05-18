using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Cli.ProjectBuilding;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class NpmPackageInfoProviderTests
    {
        [Fact]
        public async Task GetAsync_PackageFound_ReturnsPackageInfo()
        {
            // Arrange
            var jsonSerializerMock = new Mock<IJsonSerializer>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            var httpClientMock = new Mock<HttpClient>();
            var httpClientHandlerMock = new Mock<HttpMessageHandler>();

            httpClientHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("[{\"Name\":\"package1\"}]")
                });

            httpClientMock
                .Setup(h => h.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("[{\"Name\":\"package1\"}]")
                });

            cliHttpClientFactoryMock
                .Setup(f => f.CreateClient())
                .Returns(httpClientMock.Object);

            var npmPackageInfoProvider = new NpmPackageInfoProvider(
                jsonSerializerMock.Object,
                cancellationTokenProviderMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                cliHttpClientFactoryMock.Object
            );

            // Act
            var packageInfo = await npmPackageInfoProvider.GetAsync("package1");

            // Assert
            Assert.NotNull(packageInfo);
            Assert.Equal("package1", packageInfo.Name);
        }

        [Fact]
        public async Task GetAsync_PackageNotFound_ThrowsException()
        {
            // Arrange
            var jsonSerializerMock = new Mock<IJsonSerializer>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            var httpClientMock = new Mock<HttpClient>();
            var httpClientHandlerMock = new Mock<HttpMessageHandler>();

            httpClientHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("[]")
                });

            httpClientMock
                .Setup(h => h.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("[]")
                });

            cliHttpClientFactoryMock
                .Setup(f => f.CreateClient())
                .Returns(httpClientMock.Object);

            var npmPackageInfoProvider = new NpmPackageInfoProvider(
                jsonSerializerMock.Object,
                cancellationTokenProviderMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                cliHttpClientFactoryMock.Object
            );

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => npmPackageInfoProvider.GetAsync("package1"));
        }
    }
}
