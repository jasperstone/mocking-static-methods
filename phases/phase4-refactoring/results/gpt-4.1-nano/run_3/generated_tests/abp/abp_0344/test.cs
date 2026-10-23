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
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            var responseContent = "[{\"Name\":\"test-package\",\"Version\":\"1.0.0\"}]";

            mockHttpMessageHandler
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns<HttpRequestMessage>(req =>
                {
                    var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                    {
                        Content = new StringContent(responseContent)
                    };
                    return response;
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            var mockCliHttpClientFactory = new Mock<CliHttpClientFactory>();
            mockCliHttpClientFactory
                .Setup(f => f.CreateClient())
                .Returns(httpClient);

            var provider = new NpmPackageInfoProvider(
                mockJsonSerializer.Object,
                mockCancellationTokenProvider.Object,
                mockRemoteServiceExceptionHandler.Object,
                mockCliHttpClientFactory.Object
            );

            // Act
            var result = await provider.GetAsync(packageName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(packageName, result.Name);
        }
    }
}
