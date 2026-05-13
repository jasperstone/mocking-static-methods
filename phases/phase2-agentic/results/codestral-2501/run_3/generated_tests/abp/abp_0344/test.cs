using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.Volo.Abp.Cli.ProjectBuilding
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
            _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>(MockBehavior.Strict, new Mock<IHttpClientFactory>().Object, _cancellationTokenProviderMock.Object);

            _npmPackageInfoProvider = new NpmPackageInfoProvider(
                _jsonSerializerMock.Object,
                _cancellationTokenProviderMock.Object,
                _remoteServiceExceptionHandlerMock.Object,
                _cliHttpClientFactoryMock.Object);
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

            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            httpMessageHandlerMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("[{\"Name\":\"Package1\"},{\"Name\":\"Package2\"}]")
                });

            var httpClient = new HttpClient(httpMessageHandlerMock.Object);
            _cliHttpClientFactoryMock.Setup(f => f.CreateClient()).Returns(httpClient);

            _jsonSerializerMock.Setup(j => j.Deserialize<List<NpmPackageInfo>>(It.IsAny<string>()))
                .Returns(expectedPackages);

            // Act
            var result = await _npmPackageInfoProvider.GetPackageListAsync();

            // Assert
            Assert.Equal(expectedPackages, result);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnPackage_WhenPackageExists()
        {
            // Arrange
            var packageName = "Package1";
            var expectedPackage = new NpmPackageInfo { Name = packageName };

            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            httpMessageHandlerMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent($"[{{ \"Name\": \"{packageName}\" }}]")
                });

            var httpClient = new HttpClient(httpMessageHandlerMock.Object);
            _cliHttpClientFactoryMock.Setup(f => f.CreateClient()).Returns(httpClient);

            _jsonSerializerMock.Setup(j => j.Deserialize<List<NpmPackageInfo>>(It.IsAny<string>()))
                .Returns(new List<NpmPackageInfo> { expectedPackage });

            // Act
            var result = await _npmPackageInfoProvider.GetAsync(packageName);

            // Assert
            Assert.Equal(expectedPackage, result);
        }

        [Fact]
        public async Task GetAsync_ShouldThrowException_WhenPackageDoesNotExist()
        {
            // Arrange
            var packageName = "NonExistentPackage";

            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            httpMessageHandlerMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("[]")
                });

            var httpClient = new HttpClient(httpMessageHandlerMock.Object);
            _cliHttpClientFactoryMock.Setup(f => f.CreateClient()).Returns(httpClient);

            _jsonSerializerMock.Setup(j => j.Deserialize<List<NpmPackageInfo>>(It.IsAny<string>()))
                .Returns(new List<NpmPackageInfo>());

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _npmPackageInfoProvider.GetAsync(packageName));
        }
    }
}
