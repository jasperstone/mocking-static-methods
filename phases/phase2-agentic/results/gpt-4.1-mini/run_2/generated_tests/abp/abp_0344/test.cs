using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Json;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.Volo.Abp.Cli.ProjectBuilding
{
    public class NpmPackageInfoProviderTests
    {
        [Fact]
        public async Task GetPackageListAsync_Should_Call_HttpClient_GetAsync_And_Return_Deserialized_List()
        {
            // Arrange
            var expectedUrl = "https://www.abp.io/api/download/npmPackages/";
            var token = CancellationToken.None;

            var npmPackagesJson = "[{\"Name\":\"package1\"},{\"Name\":\"package2\"}]";

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
                   Content = new StringContent(npmPackagesJson),
               })
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>(MockBehavior.Strict, null, null);
            cliHttpClientFactoryMock
                .Setup(f => f.CreateClient(It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<string>()))
                .Returns(httpClient);

            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            cancellationTokenProviderMock.SetupGet(c => c.Token).Returns(token);

            var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            remoteServiceExceptionHandlerMock
                .Setup(h => h.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
                .Returns(Task.CompletedTask);

            var jsonSerializerMock = new Mock<IJsonSerializer>();
            jsonSerializerMock
                .Setup(s => s.Deserialize<List<NpmPackageInfo>>(npmPackagesJson))
                .Returns(new List<NpmPackageInfo>
                {
                    new NpmPackageInfo { Name = "package1" },
                    new NpmPackageInfo { Name = "package2" }
                });

            var provider = new NpmPackageInfoProvider(
                jsonSerializerMock.Object,
                cancellationTokenProviderMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                cliHttpClientFactoryMock.Object);

            // Act
            var result = await provider.GetPackageListAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Collection(result,
                item => Assert.Equal("package1", item.Name),
                item => Assert.Equal("package2", item.Name));

            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>());

            remoteServiceExceptionHandlerMock.Verify(h => h.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()), Times.Once);
            jsonSerializerMock.Verify(s => s.Deserialize<List<NpmPackageInfo>>(npmPackagesJson), Times.Once);
        }

        [Fact]
        public async Task GetAsync_Should_Return_Package_When_Found()
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
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>(MockBehavior.Strict, null, null);

            var providerMock = new Mock<NpmPackageInfoProvider>(
                jsonSerializerMock.Object,
                cancellationTokenProviderMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                cliHttpClientFactoryMock.Object)
            { CallBase = true };

            providerMock
                .Setup(p => p.GetPackageListAsync())
                .ReturnsAsync(packageList);

            // Act
            var result = await providerMock.Object.GetAsync(packageName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(packageName, result.Name);
        }

        [Fact]
        public async Task GetAsync_Should_Throw_Exception_When_Package_Not_Found()
        {
            // Arrange
            var packageName = "nonexistent";
            var packageList = new List<NpmPackageInfo>
            {
                new NpmPackageInfo { Name = "package1" },
                new NpmPackageInfo { Name = "package2" }
            };

            var jsonSerializerMock = new Mock<IJsonSerializer>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>(MockBehavior.Strict, null, null);

            var providerMock = new Mock<NpmPackageInfoProvider>(
                jsonSerializerMock.Object,
                cancellationTokenProviderMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                cliHttpClientFactoryMock.Object)
            { CallBase = true };

            providerMock
                .Setup(p => p.GetPackageListAsync())
                .ReturnsAsync(packageList);

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
