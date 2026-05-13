using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Core;
using Volo.Abp.Cli.Core.ProjectBuilding;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class NpmPackageInfoProviderTests
    {
        private readonly Mock<IJsonSerializer> _jsonSerializerMock;
        private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
        private readonly Mock<IRemoteServiceExceptionHandler> _remoteServiceExceptionHandlerMock;
        private readonly Mock<ICliHttpClientFactory> _cliHttpClientFactoryMock;
        private readonly Mock<HttpClient> _httpClientMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

        public NpmPackageInfoProviderTests()
        {
            _jsonSerializerMock = new Mock<IJsonSerializer>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            _cliHttpClientFactoryMock = new Mock<ICliHttpClientFactory>();
            _httpClientMock = new Mock<HttpClient>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        }

        [Fact]
        public async Task GetAsync_PackageFound_ReturnsPackageInfo()
        {
            // Arrange
            var packageList = new List<NpmPackageInfo> { new NpmPackageInfo { Name = "test-package" } };
            _jsonSerializerMock.Setup(x => x.Deserialize<List<NpmPackageInfo>>(It.IsAny<string>())).Returns(packageList);
            _cliHttpClientFactoryMock.Setup(x => x.CreateClient()).Returns(_httpClientMock.Object);
            _httpClientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[{\"Name\":\"test-package\"}]") });
            _remoteServiceExceptionHandlerMock.Setup(x => x.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var provider = new NpmPackageInfoProvider(_jsonSerializerMock.Object, _cancellationTokenProviderMock.Object,
                _remoteServiceExceptionHandlerMock.Object, _cliHttpClientFactoryMock.Object);

            // Act
            var result = await provider.GetAsync("test-package");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test-package", result.Name);
        }

        [Fact]
        public async Task GetAsync_PackageNotFound_ThrowsException()
        {
            // Arrange
            var packageList = new List<NpmPackageInfo>();
            _jsonSerializerMock.Setup(x => x.Deserialize<List<NpmPackageInfo>>(It.IsAny<string>())).Returns(packageList);
            _cliHttpClientFactoryMock.Setup(x => x.CreateClient()).Returns(_httpClientMock.Object);
            _httpClientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[]") });
            _remoteServiceExceptionHandlerMock.Setup(x => x.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var provider = new NpmPackageInfoProvider(_jsonSerializerMock.Object, _cancellationTokenProviderMock.Object,
                _remoteServiceExceptionHandlerMock.Object, _cliHttpClientFactoryMock.Object);

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => provider.GetAsync("test-package"));
        }
    }
}
