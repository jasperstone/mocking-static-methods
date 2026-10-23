using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Version;
using Volo.Abp.Http;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.Tests.ProjectBuilding
{
    public class AbpIoSourceCodeStoreTests
    {
        private readonly Mock<IJsonSerializer> _jsonSerializerMock;
        private readonly Mock<IRemoteServiceExceptionHandler> _remoteServiceExceptionHandlerMock;
        private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
        private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
        private readonly Mock<CliVersionService> _cliVersionServiceMock;
        private readonly AbpIoSourceCodeStore _sourceCodeStore;

        public AbpIoSourceCodeStoreTests()
        {
            _jsonSerializerMock = new Mock<IJsonSerializer>();
            _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            _cliVersionServiceMock = new Mock<CliVersionService>();

            _sourceCodeStore = new AbpIoSourceCodeStore(
                null,
                _jsonSerializerMock.Object,
                _remoteServiceExceptionHandlerMock.Object,
                _cancellationTokenProviderMock.Object,
                _cliHttpClientFactoryMock.Object,
                _cliVersionServiceMock.Object);
        }

        [Fact]
        public async Task IsVersionExists_ShouldReturnTrue_WhenVersionExists()
        {
            // Arrange
            var templateName = "LeptonX";
            var version = "1.0.0";
            var url = $"{CliUrls.WwwAbpIo}api/download/all-versions?includePreReleases=true";
            var responseContent = "{\"LeptonXVersions\":[{\"Name\":\"1.0.0\"}]}";

            var httpClientMock = new Mock<HttpClient>();
            httpClientMock.Setup(client => client.GetAsync(url, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    Content = new StringContent(responseContent)
                });

            _cliHttpClientFactoryMock.Setup(factory => factory.CreateClient())
                .Returns(httpClientMock.Object);

            // Act
            var result = await _sourceCodeStore.IsVersionExists(templateName, version);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsVersionExists_ShouldReturnFalse_WhenVersionDoesNotExist()
        {
            // Arrange
            var templateName = "LeptonX";
            var version = "2.0.0";
            var url = $"{CliUrls.WwwAbpIo}api/download/all-versions?includePreReleases=true";
            var responseContent = "{\"LeptonXVersions\":[{\"Name\":\"1.0.0\"}]}";

            var httpClientMock = new Mock<HttpClient>();
            httpClientMock.Setup(client => client.GetAsync(url, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    Content = new StringContent(responseContent)
                });

            _cliHttpClientFactoryMock.Setup(factory => factory.CreateClient())
                .Returns(httpClientMock.Object);

            // Act
            var result = await _sourceCodeStore.IsVersionExists(templateName, version);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsVersionExists_ShouldReturnTrue_WhenExceptionOccurs()
        {
            // Arrange
            var templateName = "LeptonX";
            var version = "1.0.0";
            var url = $"{CliUrls.WwwAbpIo}api/download/all-versions?includePreReleases=true";

            var httpClientMock = new Mock<HttpClient>();
            httpClientMock.Setup(client => client.GetAsync(url, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            _cliHttpClientFactoryMock.Setup(factory => factory.CreateClient())
                .Returns(httpClientMock.Object);

            // Act
            var result = await _sourceCodeStore.IsVersionExists(templateName, version);

            // Assert
            Assert.True(result);
        }
    }
}
