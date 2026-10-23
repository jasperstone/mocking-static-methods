using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Moq;
using Moq.Protected;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Http;
using Volo.Abp.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.Volo.Abp.Cli.ProjectBuilding
{
    // Minimal stub classes to allow compilation
    public class GithubReleaseVersion
    {
        public string Name { get; set; }
    }

    public class GithubReleaseVersions
    {
        public GithubReleaseVersion[] LeptonXVersions { get; set; }
        public GithubReleaseVersion[] FrameworkAndCommercialVersions { get; set; }
    }

    public class CliVersionService
    {
        public virtual Task<NuGet.Versioning.SemanticVersion> GetCurrentCliVersionAsync() =>
            Task.FromResult(NuGet.Versioning.SemanticVersion.Parse("1.0.0"));
    }

    public class AbpCliOptions { }

    public class AbpIoSourceCodeStoreTests
    {
        [Fact]
        public async Task IsVersionExists_ReturnsTrue_WhenVersionExistsInLeptonXVersions()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(@"{""LeptonXVersions"":[{""Name"":""1.0.0""}],""FrameworkAndCommercialVersions"":[]}")
                });

            var httpClient = new HttpClient(handlerMock.Object);

            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>(MockBehavior.Strict, (object)null);
            cliHttpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>(), It.IsAny<TimeSpan?>())).Returns(httpClient);
            cliHttpClientFactoryMock.Setup(f => f.GetCancellationToken(It.IsAny<TimeSpan>())).Returns(CancellationToken.None);

            var jsonSerializerMock = new Mock<IJsonSerializer>();
            jsonSerializerMock.Setup(s => s.Deserialize<GithubReleaseVersions>(It.IsAny<string>()))
                .Returns((string json) =>
                {
                    return new GithubReleaseVersions
                    {
                        LeptonXVersions = new[] { new GithubReleaseVersion { Name = "1.0.0" } },
                        FrameworkAndCommercialVersions = Array.Empty<GithubReleaseVersion>()
                    };
                });

            var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            remoteServiceExceptionHandlerMock.Setup(h => h.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
                .Returns(Task.CompletedTask);

            var cliVersionService = new CliVersionService();

            var optionsMock = new Mock<IOptions<AbpCliOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new AbpCliOptions());

            var store = new AbpIoSourceCodeStore(
                optionsMock.Object,
                jsonSerializerMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                null,
                cliHttpClientFactoryMock.Object,
                cliVersionService)
            {
                Logger = NullLogger<AbpIoSourceCodeStore>.Instance
            };

            var method = typeof(AbpIoSourceCodeStore).GetMethod("IsVersionExists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var result = await (Task<bool>)method.Invoke(store, new object[] { "LeptonXTemplate", "1.0.0" });

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsVersionExists_ReturnsTrue_WhenExceptionThrown()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            var httpClient = new HttpClient(handlerMock.Object);

            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>(MockBehavior.Strict, (object)null);
            cliHttpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>(), It.IsAny<TimeSpan?>())).Returns(httpClient);
            cliHttpClientFactoryMock.Setup(f => f.GetCancellationToken(It.IsAny<TimeSpan>())).Returns(CancellationToken.None);

            var jsonSerializerMock = new Mock<IJsonSerializer>();

            var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();

            var cliVersionService = new CliVersionService();

            var optionsMock = new Mock<IOptions<AbpCliOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new AbpCliOptions());

            var store = new AbpIoSourceCodeStore(
                optionsMock.Object,
                jsonSerializerMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                null,
                cliHttpClientFactoryMock.Object,
                cliVersionService)
            {
                Logger = NullLogger<AbpIoSourceCodeStore>.Instance
            };

            var method = typeof(AbpIoSourceCodeStore).GetMethod("IsVersionExists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var result = await (Task<bool>)method.Invoke(store, new object[] { "AnyTemplate", "1.0.0" });

            // Assert
            Assert.True(result);
        }
    }
}
