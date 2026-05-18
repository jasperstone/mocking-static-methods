using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library.Utility;
using Moq;
using Moq.Protected;
using Xunit;

namespace Duplicati.Library.Utility.Tests;

public class HttpClientExtensionsTests
{
    [Fact]
    public async Task UploadStream_CallsSendAsyncWithResponseContentRead()
    {
        // Arrange
        var mockMessageHandler = new Mock<HttpMessageHandler>();
        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK))
            .Verifiable();

        var httpClient = new HttpClient(mockMessageHandler.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        // Act
        var result = await httpClient.UploadStream(request, CancellationToken.None);

        // Assert
        mockMessageHandler.Protected()
            .Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req == request),
                ItExpr.IsAny<CancellationToken>());
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task UploadStream_PropagatesCancellationToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var mockMessageHandler = new Mock<HttpMessageHandler>();
        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, token) =>
            {
                Assert.True(cts.Token.CanEqual(token));
            })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var httpClient = new HttpClient(mockMessageHandler.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        // Act
        var result = await httpClient.UploadStream(request, cts.Token);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task UploadStream_HandlesNonSuccessStatusCode()
    {
        // Arrange
        var mockMessageHandler = new Mock<HttpMessageHandler>();
        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));

        var httpClient = new HttpClient(mockMessageHandler.Object);
        var request = new HttpRequestMessage(HttpMethod.Post, "https://example.com");

        // Act
        var result = await httpClient.UploadStream(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }
}
