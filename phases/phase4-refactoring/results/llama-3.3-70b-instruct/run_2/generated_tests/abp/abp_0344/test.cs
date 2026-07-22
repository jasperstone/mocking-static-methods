using System;
using System.Collections.Generic;
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
        private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
        private readonly Mock<IRemoteServiceExceptionHandler> _remoteServiceExceptionHandlerMock;
        private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
        private readonly NpmPackageInfoProvider _npmPackageInfoProvider;

        public NpmPackageInfoProviderTests()
        {
            _jsonSerializerMock = new Mock<IJsonSerializer>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            _npmPackageInfoProvider = new NpmPackageInfoProvider(
                _jsonSerializerMock.Object,
                _cancellationTokenProviderMock.Object,
                _remoteServiceExceptionHandlerMock.Object,
                _cliHttpClientFactoryMock.Object);
        }

        [Fact]
        public async Task GetAsync_PackageFound_ReturnsPackage()
        {
            // Arrange
            var packageList = new List<NpmPackageInfo>
            {
                new NpmPackageInfo { Name = "package1" },
                new NpmPackageInfo { Name = "package2" }
            };

            _jsonSerializerMock.Setup(x => x.Deserialize<List<NpmPackageInfo>>(It.IsAny<string>()))
                .Returns(packageList);

            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new Mock<HttpResponseMessage>();
            httpResponseMessage.Setup(x => x.IsSuccessStatusCode).Returns(true);
            httpResponseMessage.Setup(x => x.Content.ReadAsStringAsyncAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync("[]");

            _cliHttpClientFactoryMock.Setup(x => x.CreateClient())
                .Returns(httpClient.Object);

            httpClient.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(httpResponseMessage.Object);

            // Act
            var package = await _npmPackageInfoProvider.GetAsync("package1");

            // Assert
            Assert.NotNull(package);
            Assert.Equal("package1", package.Name);
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

            _jsonSerializerMock.Setup(x => x.Deserialize<List<NpmPackageInfo>>(It.IsAny<string>()))
                .Returns(packageList);

            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new Mock<HttpResponseMessage>();
            httpResponseMessage.Setup(x => x.IsSuccessStatusCode).Returns(true);
            httpResponseMessage.Setup(x => x.Content.ReadAsStringAsyncAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync("[]");

            _cliHttpClientFactoryMock.Setup(x => x.CreateClient())
                .Returns(httpClient.Object);

            httpClient.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(httpResponseMessage.Object);

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => _npmPackageInfoProvider.GetAsync("package3"));
        }
    }
}
