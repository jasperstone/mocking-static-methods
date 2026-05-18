using System;
using System.Collections.Generic;
using System.Net;
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
        public async Task GetPackageListAsync_ValidResponse_ReturnsPackageList()
        {
            // Arrange
            var jsonSerializer = new Mock<IJsonSerializer>();
            var cancellationTokenProvider = new Mock<ICancellationTokenProvider>();
            var remoteServiceExceptionHandler = new Mock<IRemoteServiceExceptionHandler>();
            var cliHttpClientFactory = new Mock<ICliHttpClientFactory>();
            var handlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(handlerMock.Object);

            cliHttpClientFactory.Setup(f => f.CreateClient()).Returns(httpClient);

            var packageList = new List<NpmPackageInfo>
            {
                new NpmPackageInfo { Name = "package1" },
                new NpmPackageInfo { Name = "package2" }
            };

            jsonSerializer.Setup(s => s.Deserialize<List<NpmPackageInfo>>(It.IsAny<string>())).Returns(packageList);

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("[{\"Name\":\"package1\"},{\"Name\":\"package2\"}]")
                });

            var provider = new NpmPackageInfoProvider(
                jsonSerializer.Object,
                cancellationTokenProvider.Object,
                remoteServiceExceptionHandler.Object,
                cliHttpClientFactory.Object
            );

            // Act
            var result = await provider.GetPackageListAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetAsync_PackageFound_ReturnsPackage()
        {
            // Arrange
            var jsonSerializer = new Mock<IJsonSerializer>();
            var cancellationTokenProvider = new Mock<ICancellationTokenProvider>();
            var remoteServiceExceptionHandler = new Mock<IRemoteServiceExceptionHandler>();
            var cliHttpClientFactory = new Mock<ICliHttpClientFactory>();
            var handlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(handlerMock.Object);

            cliHttpClientFactory.Setup(f => f.CreateClient()).Returns(httpClient);

            var packageList = new List<NpmPackageInfo>
            {
                new NpmPackageInfo { Name = "package1" },
                new NpmPackageInfo { Name = "package2" }
            };

            jsonSerializer.Setup(s => s.Deserialize<List<NpmPackageInfo>>(It.IsAny<string>())).Returns(packageList);

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("[{\"Name\":\"package1\"},{\"Name\":\"package2\"}]")
                });

            var provider = new NpmPackageInfoProvider(
                jsonSerializer.Object,
                cancellationTokenProvider.Object,
                remoteServiceExceptionHandler.Object,
                cliHttpClientFactory.Object
            );

            // Act
            var result = await provider.GetAsync("package1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("package1", result.Name);
        }

        [Fact]
        public async Task GetAsync_PackageNotFound_ThrowsException()
        {
            // Arrange
            var jsonSerializer = new Mock<IJsonSerializer>();
            var cancellationTokenProvider = new Mock<ICancellationTokenProvider>();
            var remoteServiceExceptionHandler = new Mock<IRemoteServiceExceptionHandler>();
            var cliHttpClientFactory = new Mock<ICliHttpClientFactory>();
            var handlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(handlerMock.Object);

            cliHttpClientFactory.Setup(f => f.CreateClient()).Returns(httpClient);

            var packageList = new List<NpmPackageInfo>
            {
                new NpmPackageInfo { Name = "package1" },
                new NpmPackageInfo { Name = "package2" }
            };

            jsonSerializer.Setup(s => s.Deserialize<List<NpmPackageInfo>>(It.IsAny<string>())).Returns(packageList);

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("[{\"Name\":\"package1\"},{\"Name\":\"package2\"}]")
                });

            var provider = new NpmPackageInfoProvider(
                jsonSerializer.Object,
                cancellationTokenProvider.Object,
                remoteServiceExceptionHandler.Object,
                cliHttpClientFactory.Object
            );

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => provider.GetAsync("package3"));
        }
    }
}
