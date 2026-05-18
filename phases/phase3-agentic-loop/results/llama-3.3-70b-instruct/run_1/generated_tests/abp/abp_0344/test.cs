using System;
using System.Net.Http;
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
        private readonly Mock<HttpClient> _httpClientMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

        public NpmPackageInfoProviderTests()
        {
            _jsonSerializerMock = new Mock<IJsonSerializer>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            _httpClientMock = new Mock<HttpClient>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        }

        [Fact]
        public async Task GetAsync_WhenPackageExists_ReturnsPackageInfo()
        {
            // Arrange
            var packageList = new List<NpmPackageInfo>
            {
                new NpmPackageInfo { Name = "package1" },
                new NpmPackageInfo { Name = "package2" }
            };

            _jsonSerializerMock.Setup(js => js.Deserialize<List<NpmPackageInfo>>(It.IsAny<string>()))
                .Returns(packageList);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("[{\"Name\":\"package1\"},{\"Name\":\"package2\"}]")
                });

            _httpClientMock = new Mock<HttpClient>(_httpMessageHandlerMock.Object);
            _cliHttpClientFactoryMock.Setup(cf => cf.CreateClient()).Returns(_httpClientMock.Object);

            var provider = new NpmPackageInfoProvider(
                _jsonSerializerMock.Object,
                _cancellationTokenProviderMock.Object,
                _remoteServiceExceptionHandlerMock.Object,
                _cliHttpClientFactoryMock.Object
            );

            // Act
            var packageInfo = await provider.GetAsync("package1");

            // Assert
            Assert.NotNull(packageInfo);
            Assert.Equal("package1", packageInfo.Name);
        }

        [Fact]
        public async Task GetAsync_WhenPackageDoesNotExist_ThrowsException()
        {
            // Arrange
            var packageList = new List<NpmPackageInfo>();

            _jsonSerializerMock.Setup(js => js.Deserialize<List<NpmPackageInfo>>(It.IsAny<string>()))
                .Returns(packageList);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("[]")
                });

            _httpClientMock = new Mock<HttpClient>(_httpMessageHandlerMock.Object);
            _cliHttpClientFactoryMock.Setup(cf => cf.CreateClient()).Returns(_httpClientMock.Object);

            var provider = new NpmPackageInfoProvider(
                _jsonSerializerMock.Object,
                _cancellationTokenProviderMock.Object,
                _remoteServiceExceptionHandlerMock.Object,
                _cliHttpClientFactoryMock.Object
            );

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => provider.GetAsync("package1"));
        }

        [Fact]
        public async Task GetPackageListAsync_WhenSuccessful_ReturnsPackageList()
        {
            // Arrange
            var packageList = new List<NpmPackageInfo>
            {
                new NpmPackageInfo { Name = "package1" },
                new NpmPackageInfo { Name = "package2" }
            };

            _jsonSerializerMock.Setup(js => js.Deserialize<List<NpmPackageInfo>>(It.IsAny<string>()))
                .Returns(packageList);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("[{\"Name\":\"package1\"},{\"Name\":\"package2\"}]")
                });

            _httpClientMock = new Mock<HttpClient>(_httpMessageHandlerMock.Object);
            _cliHttpClientFactoryMock.Setup(cf => cf.CreateClient()).Returns(_httpClientMock.Object);

            var provider = new NpmPackageInfoProvider(
                _jsonSerializerMock.Object,
                _cancellationTokenProviderMock.Object,
                _remoteServiceExceptionHandlerMock.Object,
                _cliHttpClientFactoryMock.Object
            );

            // Act
            var result = await provider.GetPackageListAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }
    }
}
