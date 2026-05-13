using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Volo.Abp.Json;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ProjectBuilding;

namespace Volo.Abp.Cli.Tests
{
    public class NpmPackageInfoProviderTests
    {
        private readonly Mock<IJsonSerializer> _jsonSerializerMock;
        private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
        private readonly Mock<IRemoteServiceExceptionHandler> _remoteServiceExceptionHandlerMock;
        private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
        private readonly Mock<HttpClient> _httpClientMock;

        public NpmPackageInfoProviderTests()
        {
            _jsonSerializerMock = new Mock<IJsonSerializer>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            _httpClientMock = new Mock<HttpClient>();
        }

        [Fact]
        public async Task GetAsync_ReturnsPackage_WhenPackageExists()
        {
            // Arrange
            var packageName = "test-package";
            var packageList = new List<NpmPackageInfo>
            {
                new NpmPackageInfo { Name = packageName }
            };

            var provider = new NpmPackageInfoProvider(
                _jsonSerializerMock.Object,
                _cancellationTokenProviderMock.Object,
                _remoteServiceExceptionHandlerMock.Object,
                _cliHttpClientFactoryMock.Object);

            var responseMessageMock = new Mock<HttpResponseMessage>();
            var contentMock = new Mock<HttpContent>();
            var responseContent = "[{\"Name\":\"test-package\"}]";

            _cliHttpClientFactoryMock.Setup(f => f.CreateClient()).Returns(_httpClientMock.Object);
            _httpClientMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMessageMock.Object);
            responseMessageMock.Setup(r => r.Content).Returns(contentMock.Object);
            contentMock.Setup(c => c.ReadAsStringAsync()).ReturnsAsync(responseContent);
            _remoteServiceExceptionHandlerMock.Setup(h => h.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
                .Returns(Task.CompletedTask);
            _jsonSerializerMock.Setup(s => s.Deserialize<List<NpmPackageInfo>>(responseContent))
                .Returns(packageList);

            // Act
            var result = await provider.GetAsync(packageName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(packageName, result.Name);
        }

        [Fact]
        public async Task GetAsync_ThrowsException_WhenPackageNotFound()
        {
            // Arrange
            var provider = new NpmPackageInfoProvider(
                _jsonSerializerMock.Object,
                _cancellationTokenProviderMock.Object,
                _remoteServiceExceptionHandlerMock.Object,
                _cliHttpClientFactoryMock.Object);

            var responseMessageMock = new Mock<HttpResponseMessage>();
            var contentMock = new Mock<HttpContent>();
            var responseContent = "[]";

            _cliHttpClientFactoryMock.Setup(f => f.CreateClient()).Returns(_httpClientMock.Object);
            _httpClientMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMessageMock.Object);
            responseMessageMock.Setup(r => r.Content).Returns(contentMock.Object);
            contentMock.Setup(c => c.ReadAsStringAsync()).ReturnsAsync(responseContent);
            _remoteServiceExceptionHandlerMock.Setup(h => h.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
                .Returns(Task.CompletedTask);
            _jsonSerializerMock.Setup(s => s.Deserialize<List<NpmPackageInfo>>(responseContent))
                .Returns(new List<NpmPackageInfo>());

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () => await provider.GetAsync("nonexistent-package"));
        }

        [Fact]
        public async Task GetPackageListAsync_ReturnsListOfPackages()
        {
            // Arrange
            var expectedPackages = new List<NpmPackageInfo>
            {
                new NpmPackageInfo { Name = "pkg1" },
                new NpmPackageInfo { Name = "pkg2" }
            };
            var responseContent = "[{\"Name\":\"pkg1\"},{\"Name\":\"pkg2\"}]";

            var provider = new NpmPackageInfoProvider(
                _jsonSerializerMock.Object,
                _cancellationTokenProviderMock.Object,
                _remoteServiceExceptionHandlerMock.Object,
                _cliHttpClientFactoryMock.Object);

            var responseMessageMock = new Mock<HttpResponseMessage>();
            var contentMock = new Mock<HttpContent>();

            _cliHttpClientFactoryMock.Setup(f => f.CreateClient()).Returns(_httpClientMock.Object);
            _httpClientMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMessageMock.Object);
            responseMessageMock.Setup(r => r.Content).Returns(contentMock.Object);
            contentMock.Setup(c => c.ReadAsStringAsync()).ReturnsAsync(responseContent);
            _remoteServiceExceptionHandlerMock.Setup(h => h.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
                .Returns(Task.CompletedTask);
            _jsonSerializerMock.Setup(s => s.Deserialize<List<NpmPackageInfo>>(responseContent))
                .Returns(expectedPackages);

            // Act
            var result = await provider.GetPackageListAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("pkg1", result[0].Name);
            Assert.Equal("pkg2", result[1].Name);
        }
    }

    // Dummy class for NpmPackageInfo to compile the test
    public class NpmPackageInfo
    {
        public string Name { get; set; }
    }
}
