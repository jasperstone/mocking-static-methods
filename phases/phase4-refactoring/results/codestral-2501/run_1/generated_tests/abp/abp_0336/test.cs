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

namespace Volo.Abp.Cli.Core.Tests
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

            var responseContent = new StringContent("{\"LeptonXVersions\":[{\"Name\":\"1.0.0\"}],\"FrameworkAndCommercialVersions\":[]}");
            var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = responseContent
            };

            mockHttpClient.Setup(client => client.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMessage);

            mockHttpClientFactory.Setup(factory => factory.CreateClient(It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<string>()))
                .Returns(mockHttpClient.Object);

            var store = new AbpIoSourceCodeStore(
                null,
                mockJsonSerializer.Object,
                mockRemoteServiceExceptionHandler.Object,
                mockCancellationTokenProvider.Object,
                mockHttpClientFactory.Object,
                null);

            // Act
            var result = await store.IsVersionExists("LeptonX", "1.0.0");

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

            var responseContent = new StringContent("{\"LeptonXVersions\":[],\"FrameworkAndCommercialVersions\":[]}");
            var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = responseContent
            };

            mockHttpClient.Setup(client => client.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMessage);

            mockHttpClientFactory.Setup(factory => factory.CreateClient(It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<string>()))
                .Returns(mockHttpClient.Object);

            var store = new AbpIoSourceCodeStore(
                null,
                mockJsonSerializer.Object,
                mockRemoteServiceExceptionHandler.Object,
                mockCancellationTokenProvider.Object,
                mockHttpClientFactory.Object,
                null);

            // Act
            var result = await store.IsVersionExists("LeptonX", "1.0.0");

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

            mockHttpClient.Setup(client => client.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            mockHttpClientFactory.Setup(factory => factory.CreateClient(It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<string>()))
                .Returns(mockHttpClient.Object);

            var store = new AbpIoSourceCodeStore(
                null,
                mockJsonSerializer.Object,
                mockRemoteServiceExceptionHandler.Object,
                mockCancellationTokenProvider.Object,
                mockHttpClientFactory.Object,
                null);

            // Act
            var result = await store.IsVersionExists("LeptonX", "1.0.0");

            // Assert
            Assert.True(result);
        }
    }
}
