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
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.Volo.Abp.Cli.ProjectBuilding
{
    public class NpmPackageInfoProviderTests
    {
        private class TestCliHttpClientFactory : CliHttpClientFactory
        {
            private readonly HttpClient _client;

            public TestCliHttpClientFactory(HttpClient client)
            {
                _client = client;
            }

            public override HttpClient CreateClient()
            {
                return _client;
            }
        }

        [Fact]
        public async Task GetPackageListAsync_ReturnsDeserializedList()
        {
            // Arrange
            var expectedPackages = new List<NpmPackageInfo>
            {
                new NpmPackageInfo { Name = "package1" },
                new NpmPackageInfo { Name = "package2" }
            };
            var json = "[{\"Name\":\"package1\"},{\"Name\":\"package2\"}]";

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
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
                   Content = new StringContent(json),
               })
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            var cliHttpClientFactory = new TestCliHttpClientFactory(httpClient);

            var jsonSerializerMock = new Mock<IJsonSerializer>();
            jsonSerializerMock.Setup(s => s.Deserialize<List<NpmPackageInfo>>(json))
                .Returns(expectedPackages);

            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            cancellationTokenProviderMock.SetupGet(c => c.Token).Returns(CancellationToken.None);

            var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            remoteServiceExceptionHandlerMock.Setup(h => h.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
                .Returns(Task.CompletedTask);

            var provider = new NpmPackageInfoProvider(
                jsonSerializerMock.Object,
                cancellationTokenProviderMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                cliHttpClientFactory);

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
                   req.RequestUri.ToString().StartsWith("https://www.abp.io/api/download/npmPackages/")),
               ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task GetAsync_ReturnsPackage_WhenFound()
        {
            // Arrange
            var packageName = "package1";
            var packages = new List<NpmPackageInfo>
            {
                new NpmPackageInfo { Name = packageName },
                new NpmPackageInfo { Name = "package2" }
            };

            var jsonSerializerMock = new Mock<IJsonSerializer>();
            jsonSerializerMock.Setup(s => s.Deserialize<List<NpmPackageInfo>>(It.IsAny<string>()))
                .Returns(packages);

            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            cancellationTokenProviderMock.SetupGet(c => c.Token).Returns(CancellationToken.None);

            var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            remoteServiceExceptionHandlerMock.Setup(h => h.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
                .Returns(Task.CompletedTask);

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
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
                   Content = new StringContent("irrelevant for this test"),
               })
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            var cliHttpClientFactory = new TestCliHttpClientFactory(httpClient);

            var provider = new NpmPackageInfoProvider(
                jsonSerializerMock.Object,
                cancellationTokenProviderMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                cliHttpClientFactory);

            // Act
            var result = await provider.GetAsync(packageName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(packageName, result.Name);
        }

        [Fact]
        public async Task GetAsync_ThrowsException_WhenPackageNotFound()
        {
            // Arrange
            var packageName = "notfound";
            var packages = new List<NpmPackageInfo>
            {
                new NpmPackageInfo { Name = "package1" },
                new NpmPackageInfo { Name = "package2" }
            };

            var jsonSerializerMock = new Mock<IJsonSerializer>();
            jsonSerializerMock.Setup(s => s.Deserialize<List<NpmPackageInfo>>(It.IsAny<string>()))
                .Returns(packages);

            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            cancellationTokenProviderMock.SetupGet(c => c.Token).Returns(CancellationToken.None);

            var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            remoteServiceExceptionHandlerMock.Setup(h => h.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
                .Returns(Task.CompletedTask);

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
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
                   Content = new StringContent("irrelevant for this test"),
               })
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            var cliHttpClientFactory = new TestCliHttpClientFactory(httpClient);

            var provider = new NpmPackageInfoProvider(
                jsonSerializerMock.Object,
                cancellationTokenProviderMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                cliHttpClientFactory);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => provider.GetAsync(packageName));
            Assert.Equal("Package is not found or downloadable!", ex.Message);
        }
    }

    // Minimal stub for NpmPackageInfo to allow compilation
    public class NpmPackageInfo
    {
        public string Name { get; set; }
    }
}
