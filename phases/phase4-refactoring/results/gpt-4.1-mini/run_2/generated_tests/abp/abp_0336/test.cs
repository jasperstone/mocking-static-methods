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
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Volo.Abp.Cli.Tests.ProjectBuilding;

public class AbpIoSourceCodeStoreTests
{
    private class TestRemoteServiceExceptionHandler : IRemoteServiceExceptionHandler
    {
        public Task EnsureSuccessfulHttpResponseAsync(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Response status code does not indicate success: {(int)response.StatusCode} ({response.ReasonPhrase}).");
            }
            return Task.CompletedTask;
        }

        public Task<string> GetAbpRemoteServiceErrorAsync(HttpResponseMessage response)
        {
            return Task.FromResult(string.Empty);
        }
    }

    [Fact]
    public async Task IsVersionExists_ReturnsTrue_WhenExceptionThrown()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var httpClient = new HttpClient(handlerMock.Object);

        var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>(MockBehavior.Strict, null, null);
        cliHttpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<TimeSpan?>())).Returns(httpClient);
        cliHttpClientFactoryMock.Setup(f => f.CreateClient()).Returns(httpClient);
        cliHttpClientFactoryMock.Setup(f => f.GetCancellationToken(It.IsAny<TimeSpan>())).Returns(CancellationToken.None);

        var jsonSerializerMock = new Mock<IJsonSerializer>();
        var store = new AbpIoSourceCodeStore(
            Options.Create(new AbpCliOptions()),
            jsonSerializerMock.Object,
            new TestRemoteServiceExceptionHandler(),
            null,
            cliHttpClientFactoryMock.Object,
            null);

        // Act
        var method = typeof(AbpIoSourceCodeStore).GetMethod("IsVersionExists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var task = (Task<bool>)method.Invoke(store, new object[] { "AnyTemplate", "1.0.0" });
        var result = await task;

        // Assert
        Assert.True(result);
    }
}
