using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectModification;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.ProjectBuilding.Tests
{
    public class NpmPackageInfoProviderTests
    {
        private readonly Mock<IJsonSerializer> _jsonSerializerMock;
        private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
        private readonly Mock<IRemoteServiceExceptionHandler> _remoteServiceExceptionHandlerMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly NpmPackageInfoProvider _npmPackageInfoProvider;

        public NpmPackageInfoProviderTests()
        {
            _jsonSerializerMock = new Mock<IJsonSerializer>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();

            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>(_httpClientFactoryMock.Object, _cancellationTokenProviderMock.Object);

            _npmPackageInfoProvider = new NpmPackageInfoProvider(
                _jsonSerializerMock.Object,
                _cancellationTokenProviderMock.Object,
                _remoteServiceExceptionHandlerMock.Object,
                cliHttpClientFactoryMock.Object);
        }

        [Fact]
        public async Task GetPackageListAsync_ShouldReturnPackageList()
        {
            // Arrange
            var expectedPackages = new List<NpmPackageInfo>
            {
                new NpmPackageInfo { Name = "Package1" },
                new NpmPackageInfo { Name = "Package2" }
            };

            var httpClientMock = new Mock<HttpClient>();
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClientMock.Object);

            var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("[{\"Name\":\"Package1\"},{\"Name\":\"Package2\"}]")
            };

            httpClientMock.Setup(client => client.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMessage);

            _jsonSerializerMock.Setup(j => j.Deserialize<List<NpmPackageInfo>>(It.IsAny<string>()))
                .Returns(expectedPackages);

            // Act
            var result = await _npmPackageInfoProvider.GetPackageListAsync();

            // Assert
            Assert.Equal(expectedPackages, result);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnPackage()
        {
            // Arrange
            var expectedPackage = new NpmPackageInfo { Name = "Package1" };

            var httpClientMock = new Mock<HttpClient>();
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClientMock.Object);

            var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("[{\"Name\":\"Package1\"},{\"Name\":\"Package2\"}]")
            };

            httpClientMock.Setup(client => client.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMessage);

            _jsonSerializerMock.Setup(j => j.Deserialize<List<NpmPackageInfo>>(It.IsAny<string>()))
                .Returns(new List<NpmPackageInfo> { expectedPackage, new NpmPackageInfo { Name = "Package2" } });

            // Act
            var result = await _npmPackageInfoProvider.GetAsync("Package1");

            // Assert
            Assert.Equal(expectedPackage, result);
        }

        [Fact]
        public async Task GetAsync_ShouldThrowException_WhenPackageNotFound()
        {
            // Arrange
            var httpClientMock = new Mock<HttpClient>();
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClientMock.Object);

            var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("[{\"Name\":\"Package2\"}]")
            };

            httpClientMock.Setup(client => client.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMessage);

            _jsonSerializerMock.Setup(j => j.Deserialize<List<NpmPackageInfo>>(It.IsAny<string>()))
                .Returns(new List<NpmPackageInfo> { new NpmPackageInfo { Name = "Package2" } });

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _npmPackageInfoProvider.GetAsync("Package1"));
        }
    }
}
