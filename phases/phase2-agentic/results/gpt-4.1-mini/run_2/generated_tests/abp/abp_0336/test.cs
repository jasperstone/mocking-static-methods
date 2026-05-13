using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Http;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.Cli.Version;

namespace Volo.Abp.Cli.ProjectBuilding.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
        private readonly Mock<IJsonSerializer> _jsonSerializerMock;
        private readonly Mock<IRemoteServiceExceptionHandler> _remoteServiceExceptionHandlerMock;
        private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
        private readonly Mock<CliVersionService> _cliVersionServiceMock;
        private readonly AbpIoSourceCodeStore _sourceCodeStore;

        public AbpIoSourceCodeStoreTests()
        {
            _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>(MockBehavior.Strict, null);
            _jsonSerializerMock = new Mock<IJsonSerializer>(MockBehavior.Strict);
            _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>(MockBehavior.Strict);
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>(MockBehavior.Strict);
            _cliVersionServiceMock = new Mock<CliVersionService>(MockBehavior.Strict, null, null);

            var optionsMock = new Mock<IOptions<AbpCliOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new AbpCliOptions());

            _sourceCodeStore = new AbpIoSourceCodeStore(
                optionsMock.Object,
                _jsonSerializerMock.Object,
                _remoteServiceExceptionHandlerMock.Object,
                _cancellationTokenProviderMock.Object,
                _cliHttpClientFactoryMock.Object,
                _cliVersionServiceMock.Object);

            // Set Logger to NullLogger to avoid null reference
            _sourceCodeStore.Logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<AbpIoSourceCodeStore>.Instance;
        }

        [Fact]
        public async Task IsVersionExists_CallsHttpClientGetAsync_ReturnsExpectedResult()
        {
            // Arrange
            var templateName = "TestTemplate";
            var version = "1.0.0";

            var url = "https://abp.io/api/download/all-versions?includePreReleases=true";

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{""LeptonXVersions"":[], ""FrameworkAndCommercialVersions"":[{""Name"":""1.0.0""}]}")
            };

            var httpClientHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            httpClientHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri.ToString() == url),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse)
                .Verifiable();

            var httpClient = new HttpClient(httpClientHandlerMock.Object);

            _cliHttpClientFactoryMock
                .Setup(f => f.CreateClient(It.IsAny<TimeSpan?>()))
                .Returns(httpClient);

            _cliHttpClientFactoryMock
                .Setup(f => f.GetCancellationToken(It.IsAny<TimeSpan>()))
                .Returns(CancellationToken.None);

            _remoteServiceExceptionHandlerMock
                .Setup(h => h.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
                .Returns(Task.CompletedTask);

            _jsonSerializerMock
                .Setup(s => s.Deserialize<GithubReleaseVersions>(It.IsAny<string>()))
                .Returns<string>(json => new GithubReleaseVersions
                {
                    LeptonXVersions = Array.Empty<VersionInfo>(),
                    FrameworkAndCommercialVersions = new[] { new VersionInfo { Name = version } }
                });

            // Use reflection to invoke private method IsVersionExists
            var method = typeof(AbpIoSourceCodeStore).GetMethod("IsVersionExists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var task = (Task<bool>)method.Invoke(_sourceCodeStore, new object[] { templateName, version });
            var result = await task;

            // Assert
            Assert.True(result);

            _cliHttpClientFactoryMock.Verify(f => f.CreateClient(It.IsAny<TimeSpan?>()), Times.Once);
            _cliHttpClientFactoryMock.Verify(f => f.GetCancellationToken(It.IsAny<TimeSpan>()), Times.Once);
            _remoteServiceExceptionHandlerMock.Verify(h => h.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()), Times.Once);
            httpClientHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString() == url),
                ItExpr.IsAny<CancellationToken>());
        }
    }

    // Helper classes to mock deserialization result
    public class GithubReleaseVersions
    {
        public VersionInfo[] LeptonXVersions { get; set; }
        public VersionInfo[] FrameworkAndCommercialVersions { get; set; }
    }

    public class VersionInfo
    {
        public string Name { get; set; }
    }
}
