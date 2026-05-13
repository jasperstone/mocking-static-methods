using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Version;
using Volo.Abp.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Volo.Abp.Cli.ProjectBuilding.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        private readonly Mock<IOptions<AbpCliOptions>> _optionsMock;
        private readonly Mock<IJsonSerializer> _jsonSerializerMock;
        private readonly Mock<IRemoteServiceExceptionHandler> _remoteServiceExceptionHandlerMock;
        private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
        private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
        private readonly Mock<CliVersionService> _cliVersionServiceMock;

        public AbpIoSourceCodeStoreTests()
        {
            _optionsMock = new Mock<IOptions<AbpCliOptions>>();
            _optionsMock.Setup(o => o.Value).Returns(new AbpCliOptions());

            _jsonSerializerMock = new Mock<IJsonSerializer>();
            _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>(MockBehavior.Strict, null, null);
            _cliVersionServiceMock = new Mock<CliVersionService>(null, null, null);
        }

        [Fact]
        public async Task IsVersionExists_CallsHttpClientGetAsync_ReturnsTrueOnException()
        {
            // Arrange
            var httpClientHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            httpClientHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            var httpClient = new HttpClient(httpClientHandlerMock.Object);

            _cliHttpClientFactoryMock
                .Setup(f => f.CreateClient(It.IsAny<TimeSpan?>()))
                .Returns(httpClient);

            _cliHttpClientFactoryMock
                .Setup(f => f.GetCancellationToken(It.IsAny<TimeSpan>()))
                .Returns(CancellationToken.None);

            var store = new AbpIoSourceCodeStore(
                _optionsMock.Object,
                _jsonSerializerMock.Object,
                _remoteServiceExceptionHandlerMock.Object,
                _cancellationTokenProviderMock.Object,
                _cliHttpClientFactoryMock.Object,
                _cliVersionServiceMock.Object);

            // Act
            var result = await store.GetType()
                .GetMethod("IsVersionExists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(store, new object[] { "templateName", "1.0.0" }) as Task<bool>;

            // Await the task result
            var boolResult = await result;

            // Assert
            Assert.True(boolResult);
        }

        [Fact]
        public async Task IsVersionExists_CallsHttpClientGetAsync_ReturnsCorrectResult()
        {
            // Arrange
            var responseContent = @"{
                ""LeptonXVersions"": [{""Name"": ""1.0.0""}],
                ""FrameworkAndCommercialVersions"": [{""Name"": ""2.0.0""}]
            }";

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent)
            };

            var httpClientHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            httpClientHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

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
                .Setup(j => j.Deserialize<GithubReleaseVersions>(It.IsAny<string>()))
                .Returns((string json) =>
                {
                    return new GithubReleaseVersions
                    {
                        LeptonXVersions = new[] { new VersionInfo { Name = "1.0.0" } },
                        FrameworkAndCommercialVersions = new[] { new VersionInfo { Name = "2.0.0" } }
                    };
                });

            var store = new AbpIoSourceCodeStore(
                _optionsMock.Object,
                _jsonSerializerMock.Object,
                _remoteServiceExceptionHandlerMock.Object,
                _cancellationTokenProviderMock.Object,
                _cliHttpClientFactoryMock.Object,
                _cliVersionServiceMock.Object);

            // Act
            var isLeptonXVersionExistsTask = (Task<bool>)store.GetType()
                .GetMethod("IsVersionExists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(store, new object[] { "LeptonX", "1.0.0" });

            var isFrameworkVersionExistsTask = (Task<bool>)store.GetType()
                .GetMethod("IsVersionExists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(store, new object[] { "Framework", "2.0.0" });

            var isLeptonXVersionExists = await isLeptonXVersionExistsTask;
            var isFrameworkVersionExists = await isFrameworkVersionExistsTask;

            // Assert
            Assert.True(isLeptonXVersionExists);
            Assert.True(isFrameworkVersionExists);
        }

        // Helper classes to match deserialization targets
        private class GithubReleaseVersions
        {
            public VersionInfo[] LeptonXVersions { get; set; }
            public VersionInfo[] FrameworkAndCommercialVersions { get; set; }
        }

        private class VersionInfo
        {
            public string Name { get; set; }
        }
    }
}
