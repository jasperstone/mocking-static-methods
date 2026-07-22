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
        private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
        private readonly Mock<HttpClient> _httpClientMock;
        private readonly NpmPackageInfoProvider _npmPackageInfoProvider;

        public NpmPackageInfoProviderTests()
        {
            _jsonSerializerMock = new Mock<IJsonSerializer>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            _httpClientMock = new Mock<HttpClient>();

            _cliHttpClientFactoryMock.Setup(factory => factory.CreateClient()).Returns(_httpClientMock.Object);

            _npmPackageInfoProvider = new NpmPackageInfoProvider(
                _jsonSerializerMock.Object,
                _cancellationTokenProviderMock.Object,
                _remoteServiceExceptionHandlerMock.Object,
                _cliHttpClientFactoryMock.Object);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnPackageInfo_WhenPackageExists()
        {
            // Arrange
            var packageName = "test-package";
            var packageList = new List<NpmPackageInfo>
            {
                new NpmPackageInfo { Name = packageName }
            };
            var jsonResponse = "[{\"Name\":\"test-package\"}]";

            _httpClientMock.Setup(client => client.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    Content = new StringContent(jsonResponse)
                });

            _jsonSerializerMock.Setup(serializer => serializer.Deserialize<List<NpmPackageInfo>>(jsonResponse))
                .Returns(packageList);

            // Act
            var result = await _npmPackageInfoProvider.GetAsync(packageName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(packageName, result.Name);
        }

        [Fact]
        public async Task GetAsync_ShouldThrowException_WhenPackageDoesNotExist()
        {
            // Arrange
            var packageName = "non-existent-package";
            var packageList = new List<NpmPackageInfo>();
            var jsonResponse = "[]";

            _httpClientMock.Setup(client => client.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    Content = new StringContent(jsonResponse)
                });

            _jsonSerializerMock.Setup(serializer => serializer.Deserialize<List<NpmPackageInfo>>(jsonResponse))
                .Returns(packageList);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _npmPackageInfoProvider.GetAsync(packageName));
        }
    }
}
