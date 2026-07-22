using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.Tests.ProjectBuilding
{
    public class AbpIoSourceCodeStoreTests
    {
        [Fact]
        public async Task IsVersionExists_ShouldReturnTrue_WhenVersionExists()
        {
            // Arrange
            var mockHttpClientFactory = new Mock<CliHttpClientFactory>();
            var mockHttpClient = new Mock<HttpClient>();
            var mockJsonSerializer = new Mock<IJsonSerializer>();
            var mockRemoteServiceExceptionHandler = new Mock<IRemoteServiceExceptionHandler>();
            var mockCancellationTokenProvider = new Mock<ICancellationTokenProvider>();

            var url = "https://www.abp.io/api/download/all-versions?includePreReleases=true";
            var responseContent = new StringContent("{\"FrameworkAndCommercialVersions\":[{\"Name\":\"1.0.0\"}]}");

            mockHttpClient.Setup(client => client.GetAsync(url, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    Content = responseContent,
                    StatusCode = System.Net.HttpStatusCode.OK
                });

            mockHttpClientFactory.Setup(factory => factory.CreateClient())
                .Returns(mockHttpClient.Object);

            var store = new AbpIoSourceCodeStore(
                null,
                mockJsonSerializer.Object,
                mockRemoteServiceExceptionHandler.Object,
                mockCancellationTokenProvider.Object,
                mockHttpClientFactory.Object,
                null);

            // Act
            var result = await store.IsVersionExists("TestTemplate", "1.0.0");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsVersionExists_ShouldReturnFalse_WhenVersionDoesNotExist()
        {
            // Arrange
            var mockHttpClientFactory = new Mock<CliHttpClientFactory>();
            var mockHttpClient = new Mock<HttpClient>();
            var mockJsonSerializer = new Mock<IJsonSerializer>();
            var mockRemoteServiceExceptionHandler = new Mock<IRemoteServiceExceptionHandler>();
            var mockCancellationTokenProvider = new Mock<ICancellationTokenProvider>();

            var url = "https://www.abp.io/api/download/all-versions?includePreReleases=true";
            var responseContent = new StringContent("{\"FrameworkAndCommercialVersions\":[]}");

            mockHttpClient.Setup(client => client.GetAsync(url, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    Content = responseContent,
                    StatusCode = System.Net.HttpStatusCode.OK
                });

            mockHttpClientFactory.Setup(factory => factory.CreateClient())
                .Returns(mockHttpClient.Object);

            var store = new AbpIoSourceCodeStore(
                null,
                mockJsonSerializer.Object,
                mockRemoteServiceExceptionHandler.Object,
                mockCancellationTokenProvider.Object,
                mockHttpClientFactory.Object,
                null);

            // Act
            var result = await store.IsVersionExists("TestTemplate", "1.0.0");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsVersionExists_ShouldReturnTrue_WhenExceptionOccurs()
        {
            // Arrange
            var mockHttpClientFactory = new Mock<CliHttpClientFactory>();
            var mockHttpClient = new Mock<HttpClient>();
            var mockJsonSerializer = new Mock<IJsonSerializer>();
            var mockRemoteServiceExceptionHandler = new Mock<IRemoteServiceExceptionHandler>();
            var mockCancellationTokenProvider = new Mock<ICancellationTokenProvider>();

            var url = "https://www.abp.io/api/download/all-versions?includePreReleases=true";

            mockHttpClient.Setup(client => client.GetAsync(url, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            mockHttpClientFactory.Setup(factory => factory.CreateClient())
                .Returns(mockHttpClient.Object);

            var store = new AbpIoSourceCodeStore(
                null,
                mockJsonSerializer.Object,
                mockRemoteServiceExceptionHandler.Object,
                mockCancellationTokenProvider.Object,
                mockHttpClientFactory.Object,
                null);

            // Act
            var result = await store.IsVersionExists("TestTemplate", "1.0.0");

            // Assert
            Assert.True(result);
        }
    }
}
