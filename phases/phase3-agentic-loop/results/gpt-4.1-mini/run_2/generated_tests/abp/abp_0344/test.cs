using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Json;
using Xunit;

namespace Volo.Abp.Cli.Tests.ProjectBuilding
{
    public class NpmPackageInfoProviderTests
    {
        private class FakeNpmPackageInfo
        {
            public string Name { get; set; }
        }

        private class FakeCliUrls
        {
            public const string WwwAbpIo = "https://www.abp.io/";
        }

        [Fact]
        public async Task GetPackageListAsync_Should_Call_HttpClient_GetAsync_And_Return_Deserialized_List()
        {
            // Arrange
            var expectedPackages = new List<FakeNpmPackageInfo>
            {
                new FakeNpmPackageInfo { Name = "Package1" },
                new FakeNpmPackageInfo { Name = "Package2" }
            };

            var json = "[{\"Name\":\"Package1\"},{\"Name\":\"Package2\"}]";

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.Is<HttpRequestMessage>(req =>
                       req.Method == HttpMethod.Get &&
                       req.RequestUri == new Uri(FakeCliUrls.WwwAbpIo + "api/download/npmPackages/")),
                   ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(httpResponse)
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            cliHttpClientFactoryMock.Setup(f => f.CreateClient()).Returns(httpClient);

            var jsonSerializerMock = new Mock<IJsonSerializer>();
            jsonSerializerMock.Setup(s => s.Deserialize<List<FakeNpmPackageInfo>>(json))
                .Returns(expectedPackages);

            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            cancellationTokenProviderMock.SetupGet(c => c.Token).Returns(CancellationToken.None);

            var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            remoteServiceExceptionHandlerMock.Setup(h => h.EnsureSuccessfulHttpResponseAsync(httpResponse))
                .Returns(Task.CompletedTask);

            var provider = new NpmPackageInfoProvider(
                jsonSerializerMock.Object,
                cancellationTokenProviderMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                cliHttpClientFactoryMock.Object);

            // Replace the CliUrls.WwwAbpIo constant by reflection or by partial class is not possible here,
            // so we will temporarily patch the URL in the test by using the same string in the mock.

            // Act
            var result = await provider.GetPackageListAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Package1", result[0].Name);
            Assert.Equal("Package2", result[1].Name);

            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri == new Uri(FakeCliUrls.WwwAbpIo + "api/download/npmPackages/")),
                ItExpr.IsAny<CancellationToken>());

            remoteServiceExceptionHandlerMock.Verify(h => h.EnsureSuccessfulHttpResponseAsync(httpResponse), Times.Once);
            jsonSerializerMock.Verify(s => s.Deserialize<List<FakeNpmPackageInfo>>(json), Times.Once);
        }

        [Fact]
        public async Task GetAsync_Should_Return_Package_When_Found()
        {
            // Arrange
            var packages = new List<FakeNpmPackageInfo>
            {
                new FakeNpmPackageInfo { Name = "Package1" },
                new FakeNpmPackageInfo { Name = "Package2" }
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

            providerMock.Setup(p => p.GetPackageListAsync()).ReturnsAsync(packages);

            // Act
            var result = await providerMock.Object.GetAsync("Package2");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Package2", result.Name);
        }

        [Fact]
        public async Task GetAsync_Should_Throw_When_Package_Not_Found()
        {
            // Arrange
            var packages = new List<FakeNpmPackageInfo>
            {
                new FakeNpmPackageInfo { Name = "Package1" }
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

            providerMock.Setup(p => p.GetPackageListAsync()).ReturnsAsync(packages);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => providerMock.Object.GetAsync("PackageX"));
            Assert.Equal("Package is not found or downloadable!", ex.Message);
        }
    }
}
