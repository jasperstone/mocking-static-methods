using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Http;

namespace Volo.Abp.Cli.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        [Fact]
        public async Task IsVersionExists_Should_Call_GetAsync_And_Return_Expected_Result()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var expectedResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"leptonXVersions\":[],\"frameworkAndCommercialVersions\":[{\"name\":\"1.0.0\"}]}"),
            };

            handlerMock
               .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
               .Returns<HttpRequestMessage>(async (request) =>
               {
                   if (request.Method == HttpMethod.Get)
                   {
                       return expectedResponse;
                   }
                   return new HttpResponseMessage(HttpStatusCode.NotFound);
               });

            var httpClient = new HttpClient(handlerMock.Object);
            var factoryMock = new Mock<CliHttpClientFactory>();
            factoryMock.Setup(f => f.CreateClient(It.IsAny<string>(), It.IsAny<TimeSpan?>()))
                       .Returns(httpClient);

            var store = new AbpIoSourceCodeStore(
                options: Microsoft.Extensions.Options.Options.Create(new AbpCliOptions()),
                jsonSerializer: new Mock<IJsonSerializer>().Object,
                remoteServiceExceptionHandler: new Mock<IRemoteServiceExceptionHandler>().Object,
                cancellationTokenProvider: new Mock<ICancellationTokenProvider>().Object,
                cliHttpClientFactory: factoryMock.Object,
                cliVersionService: new Mock<CliVersionService>().Object
            );

            // Act
            var result = await store.IsVersionExists("SomeTemplate", "1.0.0");

            // Assert
            Assert.True(result);
            handlerMock.Verify(m => m.Send(It.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get)), Times.Once);
        }
    }
}
