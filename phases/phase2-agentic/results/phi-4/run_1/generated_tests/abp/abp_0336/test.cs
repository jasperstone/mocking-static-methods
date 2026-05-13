using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Json;
using Volo.Abp.Threading;

namespace Volo.Abp.Cli.Tests.ProjectBuilding
{
    public class AbpIoSourceCodeStoreTests
    {
        private readonly Mock<ICliHttpClientFactory> _cliHttpClientFactoryMock;
        private readonly Mock<IRemoteServiceExceptionHandler> _remoteServiceExceptionHandlerMock;
        private readonly Mock<IJsonSerializer> _jsonSerializerMock;
        private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
        private readonly AbpIoSourceCodeStore _abpIoSourceCodeStore;

        public AbpIoSourceCodeStoreTests()
        {
            _cliHttpClientFactoryMock = new Mock<ICliHttpClientFactory>();
            _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            _jsonSerializerMock = new Mock<IJsonSerializer>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();

            _abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                null, // Options
                _jsonSerializerMock.Object,
                _remoteServiceExceptionHandlerMock.Object,
                _cancellationTokenProviderMock.Object,
                _cliHttpClientFactoryMock.Object,
                null // CliVersionService
            );
        }

        [Fact]
        public async Task IsVersionExists_ShouldReturnTrue_WhenVersionExists()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("{\"LeptonXVersions\": [{\"Name\": \"1.0.0\"}], \"FrameworkAndCommercialVersions\": []}")
            };

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _cliHttpClientFactoryMock
                .Setup(x => x.CreateClient())
                .Returns(httpClient);

            _jsonSerializerMock
                .Setup(x => x.Deserialize<GithubReleaseVersions>(It.IsAny<string>()))
                .Returns(new GithubReleaseVersions
                {
                    LeptonXVersions = new[] { new VersionDto { Name = "1.0.0" } },
                    FrameworkAndCommercialVersions = Array.Empty<VersionDto>()
                });

            // Act
            var result = await _abpIoSourceCodeStore.IsVersionExists("LeptonX", "1.0.0");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsVersionExists_ShouldReturnFalse_WhenVersionDoesNotExist()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("{\"LeptonXVersions\": [], \"FrameworkAndCommercialVersions\": []}")
            };

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _cliHttpClientFactoryMock
                .Setup(x => x.CreateClient())
                .Returns(httpClient);

            _jsonSerializerMock
                .Setup(x => x.Deserialize<GithubReleaseVersions>(It.IsAny<string>()))
                .Returns(new GithubReleaseVersions
                {
                    LeptonXVersions = Array.Empty<VersionDto>(),
                    FrameworkAndCommercialVersions = Array.Empty<VersionDto>()
                });

            // Act
            var result = await _abpIoSourceCodeStore.IsVersionExists("LeptonX", "1.0.0");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsVersionExists_ShouldReturnTrue_WhenExceptionOccurs()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new Exception());

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _cliHttpClientFactoryMock
                .Setup(x => x.CreateClient())
                .Returns(httpClient);

            // Act
            var result = await _abpIoSourceCodeStore.IsVersionExists("LeptonX", "1.0.0");

            // Assert
            Assert.True(result);
        }
    }

    public class GithubReleaseVersions
    {
        public VersionDto[] LeptonXVersions { get; set; }
        public VersionDto[] FrameworkAndCommercialVersions { get; set; }
    }

    public class VersionDto
    {
        public string Name { get; set; }
    }
}
