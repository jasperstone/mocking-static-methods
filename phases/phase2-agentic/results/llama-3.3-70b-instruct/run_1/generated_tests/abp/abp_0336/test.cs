using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Volo.Abp.Cli.ProjectBuilding;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        [Fact]
        public async Task IsVersionExists_ValidResponse_ReturnsTrue()
        {
            // Arrange
            var httpClientFactory = new Mock<ICliHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new Mock<HttpResponseMessage>();
            var jsonSerializer = new Mock<IJsonSerializer>();

            httpClientFactory.Setup(f => f.CreateClient()).Returns(httpClient.Object);
            httpClient.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(httpResponseMessage.Object);
            httpResponseMessage.SetupGet(m => m.IsSuccessStatusCode).Returns(true);
            jsonSerializer.Setup(s => s.Deserialize<GithubReleaseVersions>(It.IsAny<string>()))
                .Returns(new GithubReleaseVersions { LeptonXVersions = new[] { new GithubReleaseVersion { Name = "1.0.0" } } });

            var abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                new Mock<IOptions<AbpCliOptions>>().Object,
                jsonSerializer.Object,
                new Mock<IRemoteServiceExceptionHandler>().Object,
                new Mock<ICancellationTokenProvider>().Object,
                httpClientFactory.Object,
                new Mock<CliVersionService>().Object);

            // Act
            var result = await abpIoSourceCodeStore.IsVersionExists("LeptonX", "1.0.0");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsVersionExists_InvalidResponse_ReturnsFalse()
        {
            // Arrange
            var httpClientFactory = new Mock<ICliHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new Mock<HttpResponseMessage>();
            var jsonSerializer = new Mock<IJsonSerializer>();

            httpClientFactory.Setup(f => f.CreateClient()).Returns(httpClient.Object);
            httpClient.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(httpResponseMessage.Object);
            httpResponseMessage.SetupGet(m => m.IsSuccessStatusCode).Returns(false);

            var abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                new Mock<IOptions<AbpCliOptions>>().Object,
                jsonSerializer.Object,
                new Mock<IRemoteServiceExceptionHandler>().Object,
                new Mock<ICancellationTokenProvider>().Object,
                httpClientFactory.Object,
                new Mock<CliVersionService>().Object);

            // Act
            var result = await abpIoSourceCodeStore.IsVersionExists("LeptonX", "1.0.0");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsVersionExists_ThrowsException_ReturnsTrue()
        {
            // Arrange
            var httpClientFactory = new Mock<ICliHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var jsonSerializer = new Mock<IJsonSerializer>();

            httpClientFactory.Setup(f => f.CreateClient()).Returns(httpClient.Object);
            httpClient.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception());

            var abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                new Mock<IOptions<AbpCliOptions>>().Object,
                jsonSerializer.Object,
                new Mock<IRemoteServiceExceptionHandler>().Object,
                new Mock<ICancellationTokenProvider>().Object,
                httpClientFactory.Object,
                new Mock<CliVersionService>().Object);

            // Act
            var result = await abpIoSourceCodeStore.IsVersionExists("LeptonX", "1.0.0");

            // Assert
            Assert.True(result);
        }
    }
}
