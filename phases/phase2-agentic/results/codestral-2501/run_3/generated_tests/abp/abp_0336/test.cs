using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.Tests.ProjectBuilding
{
    public class AbpIoSourceCodeStoreTests
    {
        private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
        private readonly Mock<IJsonSerializer> _jsonSerializerMock;
        private readonly AbpIoSourceCodeStore _abpIoSourceCodeStore;

        public AbpIoSourceCodeStoreTests()
        {
            _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>(MockBehavior.Strict, new Mock<IHttpClientFactory>().Object, new Mock<ICancellationTokenProvider>().Object);
            _jsonSerializerMock = new Mock<IJsonSerializer>();
            _abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                Mock.Of<IOptions<AbpCliOptions>>(),
                _jsonSerializerMock.Object,
                Mock.Of<IRemoteServiceExceptionHandler>(),
                Mock.Of<ICancellationTokenProvider>(),
                _cliHttpClientFactoryMock.Object,
                Mock.Of<CliVersionService>());
        }

        [Fact]
        public async Task GetAsync_ShouldReturnTemplateFile_WhenVersionExists()
        {
            // Arrange
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"Version\":\"1.0.0\"}")
                });

            var httpClient = new HttpClient(httpMessageHandlerMock.Object);
            _cliHttpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<string>())).Returns(httpClient);

            _jsonSerializerMock.Setup(x => x.Deserialize<GetVersionResultDto>(It.IsAny<string>()))
                .Returns(new GetVersionResultDto { Version = "1.0.0" });

            // Act
            var result = await _abpIoSourceCodeStore.GetAsync("TestTemplate", "TestType", "1.0.0");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("1.0.0", result.Version);
        }

        [Fact]
        public async Task GetAsync_ShouldThrowException_WhenVersionDoesNotExist()
        {
            // Arrange
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                });

            var httpClient = new HttpClient(httpMessageHandlerMock.Object);
            _cliHttpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<string>())).Returns(httpClient);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _abpIoSourceCodeStore.GetAsync("TestTemplate", "TestType", "1.0.0"));
        }
    }
}
