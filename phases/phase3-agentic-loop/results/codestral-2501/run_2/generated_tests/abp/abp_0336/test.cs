using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Cli.GitHub;
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
        private readonly Mock<IOptions<AbpCliOptions>> _optionsMock;
        private readonly Mock<IJsonSerializer> _jsonSerializerMock;
        private readonly Mock<IRemoteServiceExceptionHandler> _remoteServiceExceptionHandlerMock;
        private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
        private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
        private readonly Mock<CliVersionService> _cliVersionServiceMock;
        private readonly Mock<HttpClient> _httpClientMock;

        public AbpIoSourceCodeStoreTests()
        {
            _optionsMock = new Mock<IOptions<AbpCliOptions>>();
            _jsonSerializerMock = new Mock<IJsonSerializer>();
            _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            _cliVersionServiceMock = new Mock<CliVersionService>();
            _httpClientMock = new Mock<HttpClient>();

            _cliHttpClientFactoryMock.Setup(f => f.CreateClient()).Returns(_httpClientMock.Object);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnTemplateFile_WhenVersionExists()
        {
            // Arrange
            var store = new AbpIoSourceCodeStore(
                _optionsMock.Object,
                _jsonSerializerMock.Object,
                _remoteServiceExceptionHandlerMock.Object,
                _cancellationTokenProviderMock.Object,
                _cliHttpClientFactoryMock.Object,
                _cliVersionServiceMock.Object);

            var templateName = "TestTemplate";
            var version = "1.0.0";
            var url = $"{CliUrls.WwwAbpIo}api/download/all-versions?includePreReleases=true";

            var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("{\"LeptonXVersions\":[{\"Name\":\"1.0.0\"}],\"FrameworkAndCommercialVersions\":[]}")
            };

            _httpClientMock.Setup(client => client.GetAsync(url, It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMessage);

            _remoteServiceExceptionHandlerMock.Setup(handler => handler.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
                .Returns(Task.CompletedTask);

            _jsonSerializerMock.Setup(serializer => serializer.Deserialize<GithubReleaseVersions>(It.IsAny<string>()))
                .Returns(new GithubReleaseVersions
                {
                    LeptonXVersions = new[] { new GithubRelease { Name = "1.0.0" } },
                    FrameworkAndCommercialVersions = Array.Empty<GithubRelease>()
                });

            // Act
            var result = await store.GetAsync(templateName, "template", version);

            // Assert
            Assert.NotNull(result);
            _httpClientMock.Verify(client => client.GetAsync(url, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
