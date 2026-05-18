using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Version;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http;
using Volo.Abp.IO;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
        private readonly Mock<IRemoteServiceExceptionHandler> _remoteServiceExceptionHandlerMock;
        private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
        private readonly Mock<IJsonSerializer> _jsonSerializerMock;
        private readonly Mock<CliVersionService> _cliVersionServiceMock;
        private readonly AbpIoSourceCodeStore _abpIoSourceCodeStore;

        public AbpIoSourceCodeStoreTests()
        {
            _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _jsonSerializerMock = new Mock<IJsonSerializer>();
            _cliVersionServiceMock = new Mock<CliVersionService>();

            _abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                new Mock<IOptions<AbpCliOptions>>().Object,
                _jsonSerializerMock.Object,
                _remoteServiceExceptionHandlerMock.Object,
                _cancellationTokenProviderMock.Object,
                _cliHttpClientFactoryMock.Object,
                _cliVersionServiceMock.Object);
        }

        [Fact]
        public async Task IsVersionExists_ValidResponse_ReturnsTrue()
        {
            // Arrange
            var httpClient = new HttpClient(new Mock<HttpMessageHandler>().Object);
            _cliHttpClientFactoryMock.Setup(f => f.CreateClient()).Returns(httpClient);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
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
                .ReturnsAsync(response);

            var httpClientWithMockHandler = new HttpClient(handlerMock.Object);
            _cliHttpClientFactoryMock.Setup(f => f.CreateClient()).Returns(httpClientWithMockHandler);

            // Act
            var result = await _abpIoSourceCodeStore.IsVersionExists("template", "1.0.0");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsVersionExists_InvalidResponse_ReturnsFalse()
        {
            // Arrange
            var httpClient = new HttpClient(new Mock<HttpMessageHandler>().Object);
            _cliHttpClientFactoryMock.Setup(f => f.CreateClient()).Returns(httpClient);

            var response = new HttpResponseMessage(HttpStatusCode.NotFound);

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            var httpClientWithMockHandler = new HttpClient(handlerMock.Object);
            _cliHttpClientFactoryMock.Setup(f => f.CreateClient()).Returns(httpClientWithMockHandler);

            // Act
            var result = await _abpIoSourceCodeStore.IsVersionExists("template", "1.0.0");

            // Assert
            Assert.False(result);
        }
    }
}
