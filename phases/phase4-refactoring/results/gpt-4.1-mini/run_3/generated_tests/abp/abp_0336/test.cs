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
using Xunit;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging.Abstractions;
using Volo.Abp.Cli.Version;

namespace Volo.Abp.Cli.Tests.ProjectBuilding;

public class AbpIoSourceCodeStoreTests
{
    private class FakeCancellationTokenProvider : ICancellationTokenProvider
    {
        public CancellationToken Token => CancellationToken.None;
    }

    [Fact]
    public async Task IsVersionExists_ReturnsTrue_WhenExceptionThrown()
    {
        // Arrange
        var httpClientHandlerMock = new Mock<HttpMessageHandler>();
        httpClientHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException());

        var httpClient = new HttpClient(httpClientHandlerMock.Object);

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var cancellationTokenProvider = new FakeCancellationTokenProvider();

        var cliHttpClientFactory = new CliHttpClientFactory(httpClientFactoryMock.Object, cancellationTokenProvider);

        var jsonSerializerMock = new Mock<IJsonSerializer>();
        var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
        var cliVersionServiceMock = new Mock<CliVersionService>();
        var optionsMock = new Mock<IOptions<AbpCliOptions>>();
        optionsMock.Setup(o => o.Value).Returns(new AbpCliOptions());

        var store = new AbpIoSourceCodeStore(
            optionsMock.Object,
            jsonSerializerMock.Object,
            remoteServiceExceptionHandlerMock.Object,
            cancellationTokenProvider,
            cliHttpClientFactory,
            cliVersionServiceMock.Object);

        // Act
        var method = typeof(AbpIoSourceCodeStore).GetMethod("IsVersionExists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var task = (Task<bool>)method.Invoke(store, new object[] { "anyTemplate", "1.0.0" });
        var result = await task;

        // Assert
        Assert.True(result);
    }
}
