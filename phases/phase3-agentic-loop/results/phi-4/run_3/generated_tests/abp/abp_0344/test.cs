using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.Tests.ProjectBuilding
{
    public class NpmPackageInfoProviderTests
    {
        private readonly Mock<ICliHttpClientFactory> _cliHttpClientFactoryMock;
        private readonly Mock<IRemoteServiceExceptionHandler> _remoteServiceExceptionHandlerMock;
        private readonly Mock<IJsonSerializer> _jsonSerializerMock;
        private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
        private readonly NpmPackageInfoProvider _npmPackageInfoProvider;

        public NpmPackageInfoProviderTests()
        {
            _cliHttpClientFactoryMock = new Mock<ICliHttpClientFactory>();
            _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            _jsonSerializerMock = new Mock<IJsonSerializer>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();

            _npmPackageInfoProvider = new NpmPackageInfoProvider(
                _jsonSerializerMock.Object,
                _cancellationTokenProviderMock.Object,
                _remoteServiceExceptionHandlerMock.Object,
                _cliHttpClientFactoryMock.Object);
        }

        [Fact]
        public async Task GetAsync_WhenPackageExists_ReturnsPackage()
        {
            // Arrange
            var expectedPackage = new NpmPackageInfo { Name = "test-package" };
            var packageList = new List<NpmPackageInfo> { expectedPackage };
            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(packageList);
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            };

            var httpClientMock = new Mock<HttpClient>();
            httpClientMock.Setup(m => m.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMessage);

            _cliHttpClientFactoryMock.Setup(m => m.CreateClient())
                .Returns(httpClientMock.Object);

            _jsonSerializerMock.Setup(m => m.Deserialize<List<NpmPackageInfo>>(It.IsAny<string>()))
                .Returns(packageList);

            _remoteServiceExceptionHandlerMock.Setup(m => m.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _npmPackageInfoProvider.GetAsync("test-package");

            // Assert
            Assert.Equal(expectedPackage, result);
        }

        [Fact]
        public async Task GetAsync_WhenPackageDoesNotExist_ThrowsException()
        {
            // Arrange
            var packageList = new List<NpmPackageInfo>();
            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(packageList);
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            };

            var httpClientMock = new Mock<HttpClient>();
            httpClientMock.Setup(m => m.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMessage);

            _cliHttpClientFactoryMock.Setup(m => m.CreateClient())
                .Returns(httpClientMock.Object);

            _jsonSerializerMock.Setup(m => m.Deserialize<List<NpmPackageInfo>>(It.IsAny<string>()))
                .Returns(packageList);

            _remoteServiceExceptionHandlerMock.Setup(m => m.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
                .Returns(Task.CompletedTask);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _npmPackageInfoProvider.GetAsync("non-existent-package"));
        }
    }
}
