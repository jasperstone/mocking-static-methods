using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library;
using Moq;
using Moq.Protected;
using Xunit;

public class JsonWebHelperHttpClientTests
{
    [Fact]
    public async Task GetResponseAsync_SuccessfulResponse_ReturnsResponse()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Success")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var jsonWebHelper = new JsonWebHelperHttpClient(httpClient);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

        // Act
        var response = await jsonWebHelper.GetResponseAsync(request, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Success", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task GetResponseAsync_UnsuccessfulResponse_ThrowsException()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent("Not Found")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var jsonWebHelper = new JsonWebHelperHttpClient(httpClient);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => jsonWebHelper.GetResponseAsync(request, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None));
    }

    [Fact]
    public async Task GetResponseUncheckedAsync_SuccessfulResponse_ReturnsResponse()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Success")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var jsonWebHelper = new JsonWebHelperHttpClient(httpClient);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

        // Act
        var response = await jsonWebHelper.GetResponseUncheckedAsync(request, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Success", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task GetResponseUncheckedAsync_UnsuccessfulResponse_ThrowsException()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent("Not Found")
            })
            .Callback(() => throw new HttpRequestException());

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var jsonWebHelper = new JsonWebHelperHttpClient(httpClient);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => jsonWebHelper.GetResponseUncheckedAsync(request, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None));
    }
}
