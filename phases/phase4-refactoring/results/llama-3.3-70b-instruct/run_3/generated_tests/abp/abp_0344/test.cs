using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Json;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class NpmPackageInfoProviderTests
    {
        private readonly Mock<IJsonSerializer> _jsonSerializerMock;
        private readonly Mock<IRemoteServiceExceptionHandler> _remoteServiceExceptionHandlerMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;

        public NpmPackageInfoProviderTests()
        {
            _jsonSerializerMock = new Mock<IJsonSerializer>();
            _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        }

        [Fact]
        public async Task GetAsync_PackageFound_ReturnsPackageInfo()
        {
            // Arrange
            var packageList = new List<NpmPackageInfo>
            {
                new NpmPackageInfo { Name = "package1" },
                new NpmPackageInfo { Name = "package2" }
            };

            _jsonSerializerMock.Setup(x => x.Deserialize<List<NpmPackageInfo>>(It.IsAny<string>())).Returns(packageList);

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("[{\"Name\":\"package1\"},{\"Name\":\"package2\"}]")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var provider = new NpmPackageInfoProvider(
                _jsonSerializerMock.Object,
                _cancellationTokenProviderMock.Object,
                _remoteServiceExceptionHandlerMock.Object,
                new CliHttpClientFactory(_httpClientFactoryMock.Object, _cancellationTokenProviderMock.Object)
            );

            // Act
            var packageInfo = await provider.GetAsync("package1");

            // Assert
            Assert.NotNull(packageInfo);
            Assert.Equal("package1", packageInfo.Name);
        }

        [Fact]
        public async Task GetAsync_PackageNotFound_ThrowsException()
        {
            // Arrange
            var packageList = new List<NpmPackageInfo>
            {
                new NpmPackageInfo { Name = "package1" },
                new NpmPackageInfo { Name = "package2" }
            };

            _jsonSerializerMock.Setup(x => x.Deserialize<List<NpmPackageInfo>>(It.IsAny<string>())).Returns(packageList);

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("[{\"Name\":\"package1\"},{\"Name\":\"package2\"}]")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var provider = new NpmPackageInfoProvider(
                _jsonSerializerMock.Object,
                _cancellationTokenProviderMock.Object,
                _remoteServiceExceptionHandlerMock.Object,
                new CliHttpClientFactory(_httpClientFactoryMock.Object, _cancellationTokenProviderMock.Object)
            );

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => provider.GetAsync("package3"));
        }
    }
}
