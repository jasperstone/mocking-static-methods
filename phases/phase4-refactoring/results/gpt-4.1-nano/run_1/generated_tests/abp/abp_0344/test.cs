using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Volo.Abp.Json;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Threading;

namespace Volo.Abp.Cli.Tests
{
    public class NpmPackageInfoProviderTests
    {
        [Fact]
        public async Task GetPackageListAsync_Should_Call_HttpClient_GetAsync_And_Return_Deserialized_List()
        {
            // Arrange
            var mockHttpClient = new Mock<HttpClient>();
            var mockFactory = new Mock<CliHttpClientFactory>();
            var mockResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("[{\"Name\":\"TestPackage\"}]")
            };

            var mockContent = new Mock<HttpContent>();
            mockContent.Setup(c => c.ReadAsStringAsync()).ReturnsAsync("[{\"Name\":\"TestPackage\"}]");
            mockResponse.Content = mockContent.Object;

            mockHttpClient
                .Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse);

            mockFactory
                .Setup(f => f.CreateClient())
                .Returns(mockHttpClient.Object);

            var mockSerializer = new Mock<IJsonSerializer>();
            mockSerializer
                .Setup(s => s.Deserialize<List<NpmPackageInfo>>(It.IsAny<string>()))
                .Returns(new List<NpmPackageInfo> { new NpmPackageInfo { Name = "TestPackage" } });

            var mockTokenProvider = new Mock<ICancellationTokenProvider>();
            mockTokenProvider
                .Setup(p => p.Token)
                .Returns(CancellationToken.None);

            var mockExceptionHandler = new Mock<IRemoteServiceExceptionHandler>();
            mockExceptionHandler
                .Setup(h => h.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
                .Returns(Task.CompletedTask);

            var provider = new NpmPackageInfoProvider(
                mockSerializer.Object,
                mockTokenProvider.Object,
                mockExceptionHandler.Object,
                mockFactory.Object
            );

            // Act
            var result = await provider.GetPackageListAsync();

            // Assert
            mockHttpClient.Verify(c => c.GetAsync($"{CliUrls.WwwAbpIo}api/download/npmPackages/", It.IsAny<CancellationToken>()), Times.Once);
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("TestPackage", result[0].Name);
        }
    }

    // Dummy class to satisfy the code dependencies
    public class NpmPackageInfo
    {
        public string Name { get; set; }
    }

    // Dummy static class for URL
    public static class CliUrls
    {
        public static string WwwAbpIo => "https://abp.io/";
    }
}
