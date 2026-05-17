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
        private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
        private readonly Mock<CliVersionService> _cliVersionServiceMock;

        public AbpIoSourceCodeStoreTests()
        {
            _optionsMock = new Mock<IOptions<AbpCliOptions>>();
            _optionsMock.Setup(o => o.Value).Returns(new AbpCliOptions());

            _jsonSerializerMock = new Mock<IJsonSerializer>();
            _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>(MockBehavior.Strict, null, null, null);
            _cliVersionServiceMock = new Mock<CliVersionService>(null, null, null, null);
        }

        [Fact]
        public async Task IsVersionExists_Calls_HttpClient_GetAsync_And_Processes_Response()
        {
            // Arrange
            var templateName = "TestTemplate";
            var version = "1.0.0";

            var expectedUrl = "https://abp.io/api/download/all-versions?includePreReleases=true";

            var githubReleaseVersionsJson = @"{
                ""LeptonXVersions"": [ { ""Name"": ""1.0.0"" } ],
                ""FrameworkAndCommercialVersions"": [ { ""Name"": ""1.0.0"" } ]
            }";

            // Setup JsonSerializer to deserialize the JSON string to a GithubReleaseVersions object
            _jsonSerializerMock.Setup(js => js.Deserialize<GithubReleaseVersions>(It.IsAny<string>()))
                .Returns(new GithubReleaseVersions
                {
                    LeptonXVersions = new[] { new GithubReleaseVersion { Name = "1.0.0" } },
                    FrameworkAndCommercialVersions = new[] { new GithubReleaseVersion { Name = "1.0.0" } }
                });

            // Setup HttpClient to return a successful response with the JSON content
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(req =>
                      req.Method == HttpMethod.Get &&
                      req.RequestUri.ToString() == expectedUrl),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(githubReleaseVersionsJson),
               })
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            // Setup CliHttpClientFactory to return the HttpClient and a CancellationToken
            _cliHttpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<TimeSpan?>())).Returns(httpClient);
            _cliHttpClientFactoryMock.Setup(f => f.CreateClient()).Returns(httpClient);
            _cliHttpClientFactoryMock.Setup(f => f.GetCancellationToken(It.IsAny<TimeSpan>())).Returns(CancellationToken.None);

            // Setup RemoteServiceExceptionHandler to do nothing on EnsureSuccessfulHttpResponseAsync
            _remoteServiceExceptionHandlerMock.Setup(r => r.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
                .Returns(Task.CompletedTask);

            var store = new AbpIoSourceCodeStore(
                _optionsMock.Object,
                _jsonSerializerMock.Object,
                _remoteServiceExceptionHandlerMock.Object,
                null,
                _cliHttpClientFactoryMock.Object,
                _cliVersionServiceMock.Object)
            {
                Logger = new LoggerFactory().CreateLogger<AbpIoSourceCodeStore>()
            };

            // Use reflection to invoke the private method IsVersionExists
            var method = typeof(AbpIoSourceCodeStore).GetMethod("IsVersionExists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);

            // Act
            var task = (Task<bool>)method.Invoke(store, new object[] { templateName, version });
            var result = await task;

            // Assert
            Assert.True(result);

            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Once(),
               ItExpr.Is<HttpRequestMessage>(req =>
                   req.Method == HttpMethod.Get &&
                   req.RequestUri.ToString() == expectedUrl),
               ItExpr.IsAny<CancellationToken>());
        }

        // Helper classes to match the deserialization target
        private class GithubReleaseVersions
        {
            public GithubReleaseVersion[] LeptonXVersions { get; set; }
            public GithubReleaseVersion[] FrameworkAndCommercialVersions { get; set; }
        }

        private class GithubReleaseVersion
        {
            public string Name { get; set; }
        }
    }
}
