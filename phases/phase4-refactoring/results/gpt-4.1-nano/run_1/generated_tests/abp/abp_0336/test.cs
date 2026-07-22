using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;
using Volo.Abp.Cli.ProjectBuilding;

namespace AbpIoSourceCodeStoreTests
{
    public class IsVersionExistsTests
    {
        [Fact]
        public async Task ReturnsExpectedResult_WhenGetAsyncReturnsSuccess()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var responseContent = "{\"leptonXVersions\": [{\"name\": \"1.0.0\"}], \"frameworkAndCommercialVersions\": [{\"name\": \"2.0.0\"}]}";

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(responseContent),
               });

            var httpClient = new HttpClient(handlerMock.Object);
            var factoryMock = new Mock<CliHttpClientFactory>();
            factoryMock.Setup(f => f.CreateClient()).Returns(httpClient);

            var store = new AbpIoSourceCodeStore(
                options: null,
                jsonSerializer: null,
                remoteServiceExceptionHandler: null,
                cancellationTokenProvider: null,
                cliHttpClientFactory: factoryMock.Object,
                cliVersionService: null);

            // Act
            var result = await store.IsVersionExists("LeptonXTemplate", "1.0.0");

            // Assert
            Assert.True(result);
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}
