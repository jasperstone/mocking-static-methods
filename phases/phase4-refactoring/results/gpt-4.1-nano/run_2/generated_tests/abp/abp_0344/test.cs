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

namespace Volo.Abp.Cli.Tests
{
    public class NpmPackageInfoProviderTests
    {
        [Fact]
        public async Task GetAsync_ReturnsPackage_WhenPackageExists()
        {
            // Arrange
            var packageName = "test-package";

            var mockJsonSerializer = new Mock<IJsonSerializer>();
            var mockCancellationTokenProvider = new Mock<ICancellationTokenProvider>();
            var mockRemoteServiceExceptionHandler = new Mock<IRemoteServiceExceptionHandler>();
            var mockHttpClient = new Mock<HttpClient>();

            var mockFactory = new Mock<CliHttpClientFactory>();
            mockFactory.Setup(f => f.CreateClient()).Returns(mockHttpClient.Object);

            var provider = new NpmPackageInfoProvider(
                mockJsonSerializer.Object,
                mockCancellationTokenProvider.Object,
                mockRemoteServiceExceptionHandler.Object,
                mockFactory.Object
            );

            var expectedPackage = new NpmPackageInfo { Name = packageName };
            var packageList = new List<NpmPackageInfo> { expectedPackage };
            var jsonResponse = "[{\"Name\":\"test-package\"}]";

            var mockResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            };

            mockHttpClient.Setup(c => c.GetAsync(
                It.Is<string>(s => s.Contains("api/download/npmPackages/")),
                It.IsAny<CancellationToken>())
            ).ReturnsAsync(mockResponse);

            mockJsonSerializer.Setup(s => s.Deserialize<List<NpmPackageInfo>>(It.IsAny<string>()))
                .Returns(packageList);

            mockRemoteServiceExceptionHandler.Setup(h => h.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await provider.GetAsync(packageName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(packageName, result.Name);
            mockHttpClient.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
