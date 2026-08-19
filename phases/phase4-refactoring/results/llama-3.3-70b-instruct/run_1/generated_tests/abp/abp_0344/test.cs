using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class NpmPackageInfoProviderTests
    {
        [Fact]
        public async Task GetAsync_PackageFound_ReturnsPackageInfo()
        {
            // Arrange
            var jsonSerializer = new Mock<IJsonSerializer>();
            var cancellationTokenProvider = new Mock<ICancellationTokenProvider>();
            var remoteServiceExceptionHandler = new Mock<IRemoteServiceExceptionHandler>();
            var cliHttpClientFactory = new Mock<Volo.Abp.Cli.Http.CliHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            cliHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<string>())).Returns(httpClient.Object);
            var provider = new NpmPackageInfoProvider(jsonSerializer.Object, cancellationTokenProvider.Object, remoteServiceExceptionHandler.Object, cliHttpClientFactory.Object);

            // Act
            var packageInfo = await provider.GetAsync("package1");

            // Assert
            Assert.NotNull(packageInfo);
            Assert.Equal("package1", packageInfo.Name);
        }

        [Fact]
        public async Task GetAsync_PackageNotFound_ThrowsException()
        {
            // Arrange
            var jsonSerializer = new Mock<IJsonSerializer>();
            var cancellationTokenProvider = new Mock<ICancellationTokenProvider>();
            var remoteServiceExceptionHandler = new Mock<IRemoteServiceExceptionHandler>();
            var cliHttpClientFactory = new Mock<Volo.Abp.Cli.Http.CliHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            cliHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<string>())).Returns(httpClient.Object);
            var provider = new NpmPackageInfoProvider(jsonSerializer.Object, cancellationTokenProvider.Object, remoteServiceExceptionHandler.Object, cliHttpClientFactory.Object);

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => provider.GetAsync("package1"));
        }
    }
}
