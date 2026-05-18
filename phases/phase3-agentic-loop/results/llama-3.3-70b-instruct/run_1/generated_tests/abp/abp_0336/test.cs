using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Templates;
using Volo.Abp.Cli.Version;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http;
using Volo.Abp.IO;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.ProjectBuilding
{
    public class AbpIoSourceCodeStoreTests
    {
        private readonly AbpIoSourceCodeStore _abpIoSourceCodeStore;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<IJsonSerializer> _jsonSerializerMock;
        private readonly Mock<IRemoteServiceExceptionHandler> _remoteServiceExceptionHandlerMock;
        private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
        private readonly Mock<CliVersionService> _cliVersionServiceMock;

        public AbpIoSourceCodeStoreTests()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _jsonSerializerMock = new Mock<IJsonSerializer>();
            _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _cliVersionServiceMock = new Mock<CliVersionService>();

            _abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                new Microsoft.Extensions.Options.OptionsWrapper<AbpCliOptions>(new AbpCliOptions()),
                _jsonSerializerMock.Object,
                _remoteServiceExceptionHandlerMock.Object,
                _cancellationTokenProviderMock.Object,
                new CliHttpClientFactory(_httpClientFactoryMock.Object, _cancellationTokenProviderMock.Object),
                _cliVersionServiceMock.Object);
        }

        [Fact]
        public async Task IsVersionExists_VersionExists_ReturnsTrue()
        {
            // Arrange
            var templateName = "Acme.BookStore";
            var version = "1.0.0";

            var httpClient = new HttpClient();
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var githubReleaseVersions = new GithubReleaseVersions
            {
                LeptonXVersions = new[] { new GithubReleaseVersion { Name = version } }
            };

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(githubReleaseVersions))
            };

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            var httpClient2 = new HttpClient(handlerMock.Object);
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient2);

            // Act
            var result = await _abpIoSourceCodeStore.IsVersionExists(templateName, version);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsVersionExists_VersionDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var templateName = "Acme.BookStore";
            var version = "1.0.0";

            var httpClient = new HttpClient();
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var githubReleaseVersions = new GithubReleaseVersions
            {
                LeptonXVersions = new GithubReleaseVersion[0]
            };

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(githubReleaseVersions))
            };

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            var httpClient2 = new HttpClient(handlerMock.Object);
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient2);

            // Act
            var result = await _abpIoSourceCodeStore.IsVersionExists(templateName, version);

            // Assert
            Assert.False(result);
        }
    }
}
