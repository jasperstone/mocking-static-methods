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
        [Fact]
        public async Task IsVersionExists_MakesGetRequestToCorrectUrl()
        {
            // Arrange
            var httpClientFactory = new Mock<ICliHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var handler = new Mock<HttpMessageHandler>();
            handler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
            httpClient
                .Setup(h => h.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
            httpClientFactory
                .Setup(f => f.CreateClient())
                .Returns(httpClient.Object);
            var abpIoSourceCodeStore = new AbpIoSourceCodeStore(
                new Mock<IOptions<AbpCliOptions>>().Object,
                new Mock<IJsonSerializer>().Object,
                new Mock<IRemoteServiceExceptionHandler>().Object,
                new Mock<ICancellationTokenProvider>().Object,
                (CliHttpClientFactory)httpClientFactory.Object,
                new Mock<CliVersionService>().Object
            );

            // Act
            await abpIoSourceCodeStore.IsVersionExists("templateName", "version");

            // Assert
            handler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once,
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                );
        }
    }
}
