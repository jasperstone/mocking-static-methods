using Moq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Http;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Volo.Abp.Cli.GitHub;
using Volo.Abp.DependencyInjection;
using Volo.Abp.IO;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        [Fact]
        public async Task IsVersionExists_ValidVersion_ReturnsTrue()
        {
            // Arrange
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var httpClientMock = new Mock<HttpClient>();
            var httpResponseMessageMock = new Mock<HttpResponseMessage>();
            var jsonSerializerMock = new Mock<IJsonSerializer>();
            var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();

            var githubReleaseVersions = new GithubReleaseVersions
            {
                LeptonXVersions = new[] { new GithubReleaseVersion { Name = "1.0.0" } },
                FrameworkAndCommercialVersions = new[] { new GithubReleaseVersion { Name = "1.0.0" } }
            };

            jsonSerializerMock.Setup(js => js.Deserialize<GithubReleaseVersions>(It.IsAny<string>())).Returns(githubReleaseVersions);

            var content = new StringContent("{\"LeptonXVersions\":[{\"Name\":\"1.0.0\"}],\"FrameworkAndCommercialVersions\":[{\"Name\":\"1.0.0\"}]}");
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            httpResponseMessageMock.Setup(h => h.Content).Returns(content);

            remoteServiceExceptionHandlerMock.Setup(r => r.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>())).Returns(Task.CompletedTask);

            httpClientMock.Setup(h => h.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(httpResponseMessageMock.Object);

            httpClientFactoryMock.Setup(h => h.CreateClient()).Returns(httpClientMock.Object);

            var abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                new Mock<IOptions<AbpCliOptions>>().Object,
                jsonSerializerMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                cancellationTokenProviderMock.Object,
                new CliHttpClientFactory(httpClientFactoryMock.Object, cancellationTokenProviderMock.Object),
                new Mock<CliVersionService>().Object
            );

            // Act
            var result = await abpIoSourceCodeStore.IsVersionExists("template", "1.0.0");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsVersionExists_InvalidVersion_ReturnsFalse()
        {
            // Arrange
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var httpClientMock = new Mock<HttpClient>();
            var httpResponseMessageMock = new Mock<HttpResponseMessage>();
            var jsonSerializerMock = new Mock<IJsonSerializer>();
            var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();

            var githubReleaseVersions = new GithubReleaseVersions
            {
                LeptonXVersions = new[] { new GithubReleaseVersion { Name = "1.0.0" } },
                FrameworkAndCommercialVersions = new[] { new GithubReleaseVersion { Name = "1.0.0" } }
            };

            jsonSerializerMock.Setup(js => js.Deserialize<GithubReleaseVersions>(It.IsAny<string>())).Returns(githubReleaseVersions);

            var content = new StringContent("{\"LeptonXVersions\":[{\"Name\":\"1.0.0\"}],\"FrameworkAndCommercialVersions\":[{\"Name\":\"1.0.0\"}]}");
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            httpResponseMessageMock.Setup(h => h.Content).Returns(content);

            remoteServiceExceptionHandlerMock.Setup(r => r.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>())).Returns(Task.CompletedTask);

            httpClientMock.Setup(h => h.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(httpResponseMessageMock.Object);

            httpClientFactoryMock.Setup(h => h.CreateClient()).Returns(httpClientMock.Object);

            var abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                new Mock<IOptions<AbpCliOptions>>().Object,
                jsonSerializerMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                cancellationTokenProviderMock.Object,
                new CliHttpClientFactory(httpClientFactoryMock.Object, cancellationTokenProviderMock.Object),
                new Mock<CliVersionService>().Object
            );

            // Act
            var result = await abpIoSourceCodeStore.IsVersionExists("template", "2.0.0");

            // Assert
            Assert.False(result);
        }
    }
}
