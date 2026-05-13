using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Http;
using Volo.Abp.Json;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.ProjectBuilding
{
    public class NpmPackageInfoProviderTests
    {
        [Fact]
        public async Task GetPackageListAsync_ShouldCallHttpClientGetAsync_AndReturnDeserializedList()
        {
            // Arrange
            var expectedUrl = "https://www.abp.io/api/download/npmPackages/";
            var token = CancellationToken.None;

            var npmPackageList = new List<NpmPackageInfo>
            {
                new NpmPackageInfo { Name = "package1" },
                new NpmPackageInfo { Name = "package2" }
            };

            var serializedJson = "[{\"Name\":\"package1\"},{\"Name\":\"package2\"}]";

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(serializedJson)
            };

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.Is<HttpRequestMessage>(req =>
                       req.Method == HttpMethod.Get &&
                       req.RequestUri == new Uri(expectedUrl)),
                   ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(httpResponse)
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            cliHttpClientFactoryMock.Setup(f => f.CreateClient()).Returns(httpClient);

            var jsonSerializerMock = new Mock<IJsonSerializer>();
            jsonSerializerMock.Setup(s => s.Deserialize<List<NpmPackageInfo>>(It.IsAny<string>()))
                .Returns(npmPackageList);

            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            cancellationTokenProviderMock.SetupGet(c => c.Token).Returns(token);

            var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            remoteServiceExceptionHandlerMock.Setup(h => h.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
                .Returns(Task.CompletedTask);

            var provider = new NpmPackageInfoProvider(
                jsonSerializerMock.Object,
                cancellationTokenProviderMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                cliHttpClientFactoryMock.Object);

            // Act
            var result = await provider.GetPackageListAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("package1", result[0].Name);
            Assert.Equal("package2", result[1].Name);

            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri == new Uri(expectedUrl)),
                ItExpr.IsAny<CancellationToken>());

            remoteServiceExceptionHandlerMock.Verify(h => h.EnsureSuccessfulHttpResponseAsync(httpResponse), Times.Once);
            jsonSerializerMock.Verify(s => s.Deserialize<List<NpmPackageInfo>>(serializedJson), Times.Once);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnPackage_WhenPackageExists()
        {
            // Arrange
            var packageName = "package1";
            var packageList = new List<NpmPackageInfo>
            {
                new NpmPackageInfo { Name = packageName },
                new NpmPackageInfo { Name = "package2" }
            };

            var jsonSerializerMock = new Mock<IJsonSerializer>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();

            var providerMock = new Mock<NpmPackageInfoProvider>(
                jsonSerializerMock.Object,
                cancellationTokenProviderMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                cliHttpClientFactoryMock.Object)
            { CallBase = true };

            providerMock.Setup(p => p.GetPackageListAsync()).ReturnsAsync(packageList);

            // Act
            var result = await providerMock.Object.GetAsync(packageName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(packageName, result.Name);
        }

        [Fact]
        public async Task GetAsync_ShouldThrowException_WhenPackageNotFound()
        {
            // Arrange
            var packageName = "nonexistent-package";
            var packageList = new List<NpmPackageInfo>
            {
                new NpmPackageInfo { Name = "package1" },
                new NpmPackageInfo { Name = "package2" }
            };

            var jsonSerializerMock = new Mock<IJsonSerializer>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();

            var providerMock = new Mock<NpmPackageInfoProvider>(
                jsonSerializerMock.Object,
                cancellationTokenProviderMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                cliHttpClientFactoryMock.Object)
            { CallBase = true };

            providerMock.Setup(p => p.GetPackageListAsync()).ReturnsAsync(packageList);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => providerMock.Object.GetAsync(packageName));
            Assert.Equal("Package is not found or downloadable!", ex.Message);
        }
    }

    // Minimal NpmPackageInfo class for testing
    public class NpmPackageInfo
    {
        public string Name { get; set; }
    }
}
