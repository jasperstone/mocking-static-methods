using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Version;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.Tests.ProjectBuilding
{
    public class AbpIoSourceCodeStoreTests
    {
        private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
        private readonly Mock<IJsonSerializer> _jsonSerializerMock;
        private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
        private readonly Mock<CliVersionService> _cliVersionServiceMock;
        private readonly AbpIoSourceCodeStore _abpIoSourceCodeStore;

        public AbpIoSourceCodeStoreTests()
        {
            _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>(MockBehavior.Strict);
            _jsonSerializerMock = new Mock<IJsonSerializer>(MockBehavior.Strict);
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>(MockBehavior.Strict);
            _cliVersionServiceMock = new Mock<CliVersionService>(MockBehavior.Strict);

            _abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                Mock.Of<IOptions<AbpCliOptions>>(),
                _jsonSerializerMock.Object,
                Mock.Of<IRemoteServiceExceptionHandler>(),
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
            var responseContent = new StringContent("{\"LeptonXVersions\":[{\"Name\":\"1.0.0\"}]}");

            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = responseContent
                });

            var httpClient = new HttpClient(httpMessageHandlerMock.Object);
            _cliHttpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<string>()))
                .Returns(httpClient);

            _cancellationTokenProviderMock.Setup(x => x.GetCancellationToken(It.IsAny<TimeSpan?>()))
                .Returns(CancellationToken.None);

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
            var responseContent = new StringContent("{\"LeptonXVersions\":[{\"Name\":\"1.0.0\"}]}");

            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = responseContent
                });

            var httpClient = new HttpClient(httpMessageHandlerMock.Object);
            _cliHttpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<string>()))
                .Returns(httpClient);

            _cancellationTokenProviderMock.Setup(x => x.GetCancellationToken(It.IsAny<TimeSpan?>()))
                .Returns(CancellationToken.None);

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
            httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exception());

            var httpClient = new HttpClient(httpMessageHandlerMock.Object);
            _cliHttpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<string>()))
                .Returns(httpClient);

            _cancellationTokenProviderMock.Setup(x => x.GetCancellationToken(It.IsAny<TimeSpan?>()))
                .Returns(CancellationToken.None);

            // Act
            var result = await _abpIoSourceCodeStore.IsVersionExists(templateName, version);

            // Assert
            Assert.True(result);
        }
    }
}
