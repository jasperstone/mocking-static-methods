using System.Net.Http.Json; // Added for deserialization
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Volo.Abp.Cli.ProjectBuilding;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
        private readonly Mock<IRemoteServiceExceptionHandler> _remoteServiceExceptionHandlerMock;
        private readonly AbpIoSourceCodeStore _abpIoSourceCodeStore;

        public AbpIoSourceCodeStoreTests()
        {
            _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            _abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                new Mock<IOptions<AbpCliOptions>>().Object,
                new Mock<IJsonSerializer>().Object,
                _remoteServiceExceptionHandlerMock.Object,
                new Mock<ICancellationTokenProvider>().Object,
                _cliHttpClientFactoryMock.Object,
                new Mock<CliVersionService>().Object);
        }

        [Fact]
        public async Task IsVersionExists_ValidResponse_ReturnsTrue()
        {
            // Arrange
            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"LeptonXVersions\":[{\"Name\":\"1.0.0\"}],\"FrameworkAndCommercialVersions\":[{\"Name\":\"1.0.0\"}]}")
            };
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponseMessage);
            var httpClientInstance = new HttpClient(handlerMock.Object);
            _cliHttpClientFactoryMock
                .Setup(f => f.CreateClient())
                .Returns(httpClientInstance);

            // Act
            var result = await _abpIoSourceCodeStore.IsVersionExists("LeptonX", "1.0.0");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsVersionExists_InvalidResponse_ReturnsFalse()
        {
            // Arrange
            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponseMessage);
            var httpClientInstance = new HttpClient(handlerMock.Object);
            _cliHttpClientFactoryMock
                .Setup(f => f.CreateClient())
                .Returns(httpClientInstance);

            // Act
            var result = await _abpIoSourceCodeStore.IsVersionExists("LeptonX", "1.0.0");

            // Assert
            Assert.False(result);
        }
    }
}
