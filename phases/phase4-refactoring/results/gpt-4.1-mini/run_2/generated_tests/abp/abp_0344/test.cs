using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Json;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.ProjectBuilding
{
    public class NpmPackageInfoProviderTests
    {
        [Fact]
        public async Task GetPackageListAsync_ReturnsDeserializedList()
        {
            // Arrange
            var expectedPackages = new List<NpmPackageInfo>
            {
                new NpmPackageInfo { Name = "TestPackage1" },
                new NpmPackageInfo { Name = "TestPackage2" }
            };

            var json = "[{\"Name\":\"TestPackage1\"},{\"Name\":\"TestPackage2\"}]";

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };

            var handler = new FakeHttpMessageHandler(httpResponse);
            var httpClient = new HttpClient(handler);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var dummyCancellationTokenProvider = new DummyCancellationTokenProvider();

            var cliHttpClientFactory = new CliHttpClientFactory(mockHttpClientFactory.Object, dummyCancellationTokenProvider);

            var mockJsonSerializer = new Mock<IJsonSerializer>();
            mockJsonSerializer.Setup(s => s.Deserialize<List<NpmPackageInfo>>(json)).Returns(expectedPackages);

            var mockRemoteServiceExceptionHandler = new Mock<IRemoteServiceExceptionHandler>();
            mockRemoteServiceExceptionHandler
                .Setup(h => h.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
                .Returns(Task.CompletedTask);

            var provider = new NpmPackageInfoProvider(
                mockJsonSerializer.Object,
                dummyCancellationTokenProvider,
                mockRemoteServiceExceptionHandler.Object,
                cliHttpClientFactory);

            // Act
            var result = await provider.GetPackageListAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("TestPackage1", result[0].Name);
            Assert.Equal("TestPackage2", result[1].Name);
        }

        [Fact]
        public async Task GetAsync_ReturnsPackage_WhenFound()
        {
            // Arrange
            var packageName = "TestPackage1";
            var packages = new List<NpmPackageInfo>
            {
                new NpmPackageInfo { Name = packageName },
                new NpmPackageInfo { Name = "OtherPackage" }
            };

            var mockJsonSerializer = new Mock<IJsonSerializer>();
            var dummyCancellationTokenProvider = new DummyCancellationTokenProvider();
            var mockRemoteServiceExceptionHandler = new Mock<IRemoteServiceExceptionHandler>();
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();

            var cliHttpClientFactory = new CliHttpClientFactory(mockHttpClientFactory.Object, dummyCancellationTokenProvider);

            var provider = new TestableNpmPackageInfoProvider(
                mockJsonSerializer.Object,
                dummyCancellationTokenProvider,
                mockRemoteServiceExceptionHandler.Object,
                cliHttpClientFactory,
                packages);

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
            var packageName = "NonExistentPackage";
            var packages = new List<NpmPackageInfo>
            {
                new NpmPackageInfo { Name = "TestPackage1" },
                new NpmPackageInfo { Name = "OtherPackage" }
            };

            var mockJsonSerializer = new Mock<IJsonSerializer>();
            var dummyCancellationTokenProvider = new DummyCancellationTokenProvider();
            var mockRemoteServiceExceptionHandler = new Mock<IRemoteServiceExceptionHandler>();
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();

            var cliHttpClientFactory = new CliHttpClientFactory(mockHttpClientFactory.Object, dummyCancellationTokenProvider);

            var provider = new TestableNpmPackageInfoProvider(
                mockJsonSerializer.Object,
                dummyCancellationTokenProvider,
                mockRemoteServiceExceptionHandler.Object,
                cliHttpClientFactory,
                packages);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => provider.GetAsync(packageName));
            Assert.Equal("Package is not found or downloadable!", ex.Message);
        }

        private class DummyCancellationTokenProvider : ICancellationTokenProvider
        {
            public CancellationToken Token => CancellationToken.None;
        }

        private class FakeHttpMessageHandler : HttpMessageHandler
        {
            private readonly HttpResponseMessage _response;

            public FakeHttpMessageHandler(HttpResponseMessage response)
            {
                _response = response;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(_response);
            }
        }

        // Subclass to override GetPackageListAsync to return a fixed list for testing GetAsync
        private class TestableNpmPackageInfoProvider : NpmPackageInfoProvider
        {
            private readonly List<NpmPackageInfo> _packageList;

            public TestableNpmPackageInfoProvider(
                IJsonSerializer jsonSerializer,
                ICancellationTokenProvider cancellationTokenProvider,
                IRemoteServiceExceptionHandler remoteServiceExceptionHandler,
                CliHttpClientFactory cliHttpClientFactory,
                List<NpmPackageInfo> packageList)
                : base(jsonSerializer, cancellationTokenProvider, remoteServiceExceptionHandler, cliHttpClientFactory)
            {
                _packageList = packageList;
            }

            public override Task<List<NpmPackageInfo>> GetPackageListAsync()
            {
                return Task.FromResult(_packageList);
            }
        }
    }

    public class NpmPackageInfo
    {
        public string Name { get; set; }
    }
}
