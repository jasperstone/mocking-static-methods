using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Http;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.ProjectBuilding.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
        private readonly Mock<IJsonSerializer> _jsonSerializerMock;
        private readonly Mock<IRemoteServiceExceptionHandler> _remoteServiceExceptionHandlerMock;
        private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
        private readonly AbpIoSourceCodeStore _abpIoSourceCodeStore;

        public AbpIoSourceCodeStoreTests()
        {
            _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>(MockBehavior.Strict);
            _jsonSerializerMock = new Mock<IJsonSerializer>(MockBehavior.Strict);
            _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>(MockBehavior.Strict);
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>(MockBehavior.Strict);

            _abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                null,
                _jsonSerializerMock.Object,
                _remoteServiceExceptionHandlerMock.Object,
                _cancellationTokenProviderMock.Object,
                _cliHttpClientFactoryMock.Object,
                null);
        }

        [Fact]
        public async Task IsVersionExists_ShouldReturnTrue_WhenVersionExists()
        {
            // Arrange
            var templateName = "LeptonX";
            var version = "1.0.0";
            var url = $"{CliUrls.WwwAbpIo}api/download/all-versions?includePreReleases=true";
            var responseContent = "{\"LeptonXVersions\":[{\"Name\":\"1.0.0\"}]}";

            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            httpMessageHandlerMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent)
                });

            var httpClient = new HttpClient(httpMessageHandlerMock.Object);
            _cliHttpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<string>())).Returns(httpClient);
            _cancellationTokenProviderMock.Setup(p => p.Token).Returns(CancellationToken.None);

            // Act
            var result = await _abpIoSourceCodeStore.IsVersionExists(templateName, version);

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

            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            httpMessageHandlerMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent)
                });

            var httpClient = new HttpClient(httpMessageHandlerMock.Object);
            _cliHttpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<string>())).Returns(httpClient);
            _cancellationTokenProviderMock.Setup(p => p.Token).Returns(CancellationToken.None);

            // Act
            var result = await _abpIoSourceCodeStore.IsVersionExists(templateName, version);

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

            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            httpMessageHandlerMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            var httpClient = new HttpClient(httpMessageHandlerMock.Object);
            _cliHttpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<string>())).Returns(httpClient);
            _cancellationTokenProviderMock.Setup(p => p.Token).Returns(CancellationToken.None);

            // Act
            var result = await _abpIoSourceCodeStore.IsVersionExists(templateName, version);

            // Assert
            Assert.True(result);
        }
    }
}
