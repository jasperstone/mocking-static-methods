using System;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        private readonly AbpIoSourceCodeStore _abpIoSourceCodeStore;
        private readonly Mock<ICliHttpClientFactory> _cliHttpClientFactoryMock;

        public AbpIoSourceCodeStoreTests()
        {
            _abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                new Mock<IOptions<AbpCliOptions>>().Object,
                new Mock<IJsonSerializer>().Object,
                new Mock<IRemoteServiceExceptionHandler>().Object,
                new Mock<ICancellationTokenProvider>().Object,
                new CliHttpClientFactory(new Mock<ILogger<CliHttpClientFactory>>().Object),
                new Mock<CliVersionService>().Object
            );
            _cliHttpClientFactoryMock = new Mock<ICliHttpClientFactory>();
        }

        [Fact]
        public async Task IsVersionExists_WithValidVersion_ReturnsTrue()
        {
            // Arrange
            var templateName = "template-name";
            var version = "1.0.0";
            var url = $"{CliUrls.WwwAbpIo}api/download/all-versions?includePreReleases=true";
            var githubReleaseVersions = new GithubReleaseVersions
            {
                LeptonXVersions = new[] { new GithubReleaseVersion { Name = version } }
            };

            var httpClientMock = new Mock<HttpClient>();
            var httpClientHandlerMock = new Mock<HttpMessageHandler>();
            httpClientHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(githubReleaseVersions))
                });

            httpClientMock
                .Setup(h => h.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(githubReleaseVersions))
                });

            _cliHttpClientFactoryMock
                .Setup(f => f.CreateClient())
                .Returns(httpClientMock.Object);

            // Act
            var result = await _abpIoSourceCodeStore.IsVersionExists(templateName, version);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsVersionExists_WithInvalidVersion_ReturnsFalse()
        {
            // Arrange
            var templateName = "template-name";
            var version = "1.0.0";
            var url = $"{CliUrls.WwwAbpIo}api/download/all-versions?includePreReleases=true";
            var githubReleaseVersions = new GithubReleaseVersions
            {
                LeptonXVersions = new[] { new GithubReleaseVersion { Name = "2.0.0" } }
            };

            var httpClientMock = new Mock<HttpClient>();
            var httpClientHandlerMock = new Mock<HttpMessageHandler>();
            httpClientHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(githubReleaseVersions))
                });

            httpClientMock
                .Setup(h => h.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(githubReleaseVersions))
                });

            _cliHttpClientFactoryMock
                .Setup(f => f.CreateClient())
                .Returns(httpClientMock.Object);

            // Act
            var result = await _abpIoSourceCodeStore.IsVersionExists(templateName, version);

            // Assert
            Assert.False(result);
        }
    }
}
