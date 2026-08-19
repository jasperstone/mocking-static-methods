using Duplicati.Library.Backend.OAuthHelper;
using Moq;
using Moq.Protected;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Duplicati.Library.Backend.OAuthHelper.Tests;

public class JsonWebHelperHttpClientTests
{
    [Fact]
    public async Task GetResponseUncheckedAsync_SuccessfulResponse_ReturnsResponse()
    {
        // Arrange
        var mockHttpClientHandler = new Mock<HttpMessageHandler>();
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("Success")
        };

        mockHttpClientHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<HttpCompletionOption>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var httpClient = new HttpClient(mockHttpClientHandler.Object);
        var helper = new JsonWebHelperHttpClient(httpClient);
        var cts = new CancellationTokenSource();

        // Act
        var result = await helper.GetResponseUncheckedAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cts.Token);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        mockHttpClientHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<HttpCompletionOption>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetResponseUncheckedAsync_HttpRequestException_PropagatesException()
    {
        // Arrange
        var mockHttpClientHandler = new Mock<HttpMessageHandler>();
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        var expectedException = new HttpRequestException("Network error");

        mockHttpClientHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<HttpCompletionOption>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(expectedException);

        var httpClient = new HttpClient(mockHttpClientHandler.Object);
        var helper = new JsonWebHelperHttpClient(httpClient);
        var cts = new CancellationTokenSource();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => helper.GetResponseUncheckedAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cts.Token));

        Assert.Equal("Network error", exception.Message);
    }

    [Fact]
    public async Task GetResponseAsync_SuccessfulResponse_ReturnsResponse()
    {
        // Arrange
        var mockHttpClientHandler = new Mock<HttpMessageHandler>();
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("Success")
        };

        mockHttpClientHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<HttpCompletionOption>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var httpClient = new HttpClient(mockHttpClientHandler.Object);
        var helper = new JsonWebHelperHttpClient(httpClient);
        var cts = new CancellationTokenSource();

        // Act
        var result = await helper.GetResponseAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cts.Token);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task GetResponseAsync_NonSuccessStatusCode_ThrowsHttpRequestException()
    {
        // Arrange
        var mockHttpClientHandler = new Mock<HttpMessageHandler>();
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Bad Request")
        };

        mockHttpClientHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<HttpCompletionOption>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var httpClient = new HttpClient(mockHttpClientHandler.Object);
        var helper = new JsonWebHelperHttpClient(httpClient);
        var cts = new CancellationTokenSource();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => helper.GetResponseAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cts.Token));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
    }
}
