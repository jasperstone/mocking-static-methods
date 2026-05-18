using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Version;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Xunit;
using Volo.Abp.Cli.ProjectBuilding.Templates.App;
using Volo.Abp.Cli.ProjectBuilding.Templates.Console;
using Volo.Abp.Cli.ProjectBuilding.Templates.Maui;
using Volo.Abp.Cli.ProjectBuilding.Templates.MvcModule;
using Volo.Abp.Cli.ProjectBuilding.Templates.Wpf;
using Volo.Abp.Cli.GitHub;
using Volo.Abp.Cli;
using Volo.Abp.Cli.ProjectBuilding.Templates;

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
            _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>(MockBehavior.Strict, new Mock<IHttpClientFactory>().Object, new Mock<ICancellationTokenProvider>().Object);
            _jsonSerializerMock = new Mock<IJsonSerializer>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _cliVersionServiceMock = new Mock<CliVersionService>();

            _abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                new Mock<IOptions<AbpCliOptions>>().Object,
                _jsonSerializerMock.Object,
                new Mock<IRemoteServiceExceptionHandler>().Object,
                _cancellationTokenProviderMock.Object,
                _cliHttpClientFactoryMock.Object,
                _cliVersionServiceMock.Object);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnVersion_WhenVersionExists()
        {
            // Arrange
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"Version\":\"1.0.0\"}")
                });

            var httpClient = new HttpClient(httpMessageHandlerMock.Object);
            _cliHttpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<string>())).Returns(httpClient);

            _jsonSerializerMock.Setup(x => x.Deserialize<GetVersionResultDto>(It.IsAny<string>()))
                .Returns(new GetVersionResultDto { Version = "1.0.0" });

            // Act
            var result = await _abpIoSourceCodeStore.GetAsync("templateName", "templateType", "1.0.0");

            // Assert
            Assert.Equal("1.0.0", result);
        }

        [Fact]
        public async Task IsVersionExists_ShouldReturnTrue_WhenVersionExists()
        {
            // Arrange
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"FrameworkAndCommercialVersions\":[{\"Name\":\"1.0.0\"}]}")
                });

            var httpClient = new HttpClient(httpMessageHandlerMock.Object);
            _cliHttpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<string>())).Returns(httpClient);

            _jsonSerializerMock.Setup(x => x.Deserialize<GithubReleaseVersions>(It.IsAny<string>()))
                .Returns(new GithubReleaseVersions
                {
                    FrameworkAndCommercialVersions = new[] { new GithubReleaseVersion { Name = "1.0.0" } }
                });

            // Act
            var result = await _abpIoSourceCodeStore.IsVersionExists("templateName", "1.0.0");

            // Assert
            Assert.True(result);
        }
    }
}
